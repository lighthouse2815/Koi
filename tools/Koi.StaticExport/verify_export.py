#!/usr/bin/env python3
"""Validate the exported website with Python's standard library; no server or database required."""
import json
import re
import sys
from html.parser import HTMLParser
from pathlib import Path
from urllib.parse import unquote, urljoin, urlsplit
from uuid import UUID


class References(HTMLParser):
    def __init__(self, filename):
        super().__init__(convert_charrefs=True)
        self.filename = filename
        self.script_list = False

    def handle_starttag(self, tag, attributes):
        values = dict(attributes)
        self.script_list = tag == "script" and values.get("id") == "static-scripts"
        if values.get("name") == "__RequestVerificationToken":
            failures.append(f"CSRF token found in {self.filename}")
        if tag == "form" and values.get("method", "get").lower() == "post":
            failures.append(f"Backend form found in {self.filename}")
        for field in ("href", "src", "poster"):
            if values.get(field):
                check(values[field], self.filename)
        for item in values.get("srcset", "").split(","):
            if item.strip():
                check(item.strip().split()[0], self.filename)
        for url in re.findall(r"url\(['\"]?([^)'\"]+)", values.get("style", "")):
            check(url, self.filename)

    def handle_data(self, data):
        if self.script_list:
            for url in json.loads(data):
                check(url, self.filename)

    def handle_endtag(self, tag):
        if tag == "script":
            self.script_list = False


root = Path(sys.argv[1] if len(sys.argv) > 1 else "fe-demo").resolve()
failures = []
checked = set()


def check(raw, context):
    if raw.startswith(("#", "data:", "mailto:", "tel:", "javascript:")):
        return
    location = urlsplit(urljoin("https://static.test/" + str(context), raw))
    if location.netloc != "static.test":
        if location.hostname in ("localhost", "127.0.0.1"):
            failures.append(f"Local development address: {raw}")
        return
    path = unquote(location.path).lstrip("/")
    if path.lower().startswith(("admin", "home/", "en/home/newspreview", "vi/home/newspreview")):
        failures.append(f"Backend route referenced: {raw}")
        return
    target = root / path
    if not target.is_file() and not (target / "index.html").is_file():
        failures.append(f"Missing reference {raw} in {context}")
    checked.add(path)


manifest = json.loads((root / "static-site.json").read_text())
assert manifest["generator"] == "Koi.StaticExport"
assert (root / "index.html").is_file() and (root / "404.html").is_file()
assert len([file for file in root.rglob("*") if file.is_file()]) <= 20000
for file in root.rglob("*"):
    if not file.is_file():
        continue
    relative = file.relative_to(root)
    if file.stat().st_size > 25 * 1024 * 1024:
        failures.append(f"Cloudflare asset exceeds 25 MiB: {relative}")
    if file.suffix in (".cs", ".cshtml", ".csproj", ".dll", ".pdb", ".resx") or file.name.startswith("appsettings") or file.name == ".env":
        failures.append(f"Server/source file in export: {relative}")
    if file.suffix == ".html":
        References(relative).feed(file.read_text())
    if file.suffix == ".css":
        for url in re.findall(r"url\(['\"]?([^)'\"]+)", file.read_text()):
            check(url, relative)

for language in manifest["languages"]:
    stores = json.loads((root / f"data/stores-{language}.json").read_text())
    news = json.loads((root / f"data/news-{language}.json").read_text())
    assert len(stores["stores"]) == manifest["counts"]["stores"]
    assert len({value["id"] for value in stores["stores"]}) == len(stores["stores"])
    assert len(news["articles"]) == max(0, manifest["counts"]["news"] - 1)
    for value in stores["stores"] + news["articles"]:
        References(Path(f"{language}/Home/Store/index.html")).feed(value["html"])
    for file in (root / language / "Home/NewsDetail").glob("*/index.html"):
        assert str(UUID(file.parent.name)) == file.parent.name
    assert len(list((root / language / "Home/NewsDetail").glob("*/index.html"))) == manifest["counts"]["news"]

if failures:
    print("\n".join(sorted(set(failures))), file=sys.stderr)
    raise SystemExit(1)
print(f"PASS static export: {manifest['counts']}; {len(checked)} local references; no server files or POST forms.")
