"""Notes fictives par classe et par matière : un niveau propre à chaque élève + un écart par matière et séquence."""
import csv
import sys
from pathlib import Path


def fnv(text: str) -> int:
    value = 2166136261
    for char in text:
        value ^= ord(char)
        value = (value * 16777619) & 0xFFFFFFFF
    return value


def note(name: str, subject: str, sequence: int) -> float:
    base = 7.5 + (fnv(name) % 950) / 100
    noise = (fnv(f"{name}|{subject}|{sequence}") % 600) / 100 - 3
    value = round((base + noise) * 2) / 2
    return max(4.0, min(19.0, value))


source, class_name, out_dir = Path(sys.argv[1]), sys.argv[2], Path(sys.argv[3])
subjects = [s for s in sys.argv[4].split(",")]
with source.open(encoding="utf-8-sig") as file:
    students = [row for row in csv.DictReader(file, delimiter=";") if row["Classe"] == class_name]
students.sort(key=lambda row: (row["Nom"], row["Prénom"]))
out_dir.mkdir(parents=True, exist_ok=True)
for subject in subjects:
    lines = ["Matricule\tNom\tSéq 1\tSéq 2"]
    for row in students:
        name = f'{row["Nom"]} {row["Prénom"]}'
        lines.append(f'{row["Matricule"]}\t{name}\t{note(name, subject, 1):g}\t{note(name, subject, 2):g}')
    (out_dir / f"{class_name.replace(' ', '')}-{subject}.tsv").write_text("\n".join(lines), encoding="utf-8")
print(len(students), "élèves,", len(subjects), "matières")
