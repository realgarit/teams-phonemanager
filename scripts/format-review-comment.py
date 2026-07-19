#!/usr/bin/env python3
"""Format Semgrep JSON + AI review text into one PR comment body.

Usage: format-review-comment.py <semgrep.json> <ai-review.txt>
Prints the formatted comment to stdout.
"""
import json
import sys


def format_semgrep(path):
    with open(path) as f:
        data = json.load(f)
    results = data.get("results", [])
    if not results:
        return "No Semgrep findings."
    lines = []
    for r in results:
        path_ = r.get("path", "?")
        line = r.get("start", {}).get("line", "?")
        severity = r.get("extra", {}).get("severity", "?")
        check_id = r.get("check_id", "?")
        message = r.get("extra", {}).get("message", "").strip()
        lines.append(f"- **{path_}:{line}** ({severity}, `{check_id}`) - {message}")
    return "\n".join(lines)


def format_ai_review(path):
    with open(path) as f:
        text = f.read().strip()
    return text if text else "No findings."


def main():
    semgrep_path, ai_review_path = sys.argv[1], sys.argv[2]
    semgrep_section = format_semgrep(semgrep_path)
    ai_section = format_ai_review(ai_review_path)
    print("## Automated review\n")
    print("### Semgrep (deterministic)\n")
    print(semgrep_section)
    print()
    print("### AI review (security + code quality)\n")
    print(ai_section)


if __name__ == "__main__":
    main()
