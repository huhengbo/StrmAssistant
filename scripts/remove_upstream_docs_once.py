from pathlib import Path

path = Path('docs/emby-4.9-compatibility-validation.md')
text = path.read_text(encoding='utf-8')
text = text.replace('- Upstream: `sjtuross/StrmAssistant`\n', '')
path.write_text(text, encoding='utf-8')
