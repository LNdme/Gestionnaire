"""Génère des élèves et enseignants entièrement fictifs pour l'établissement de démonstration."""
import csv
import random
import sys
from pathlib import Path

out = Path(sys.argv[1])
out.mkdir(parents=True, exist_ok=True)
rng = random.Random(2026)

surnames = ["ABENA", "ATANGANA", "BELLA", "BIYIK", "EBOGO", "ESSOMBA", "ETOGA", "FOUDA", "KAMGA", "KENFACK",
            "MBARGA", "MBIDA", "MENYE", "NANA", "NDZANA", "NGONO", "NJOYA", "NKOLO", "NOAH", "OWONA",
            "TAMBA", "TCHOUA", "WAMBA", "ZANGA", "YOMBI", "MOUSSA", "HAMADOU", "DJOMO", "FOTSO", "TENE"]
boys = ["Arnaud", "Brice", "Cédric", "Daniel", "Éric", "Franck", "Gaël", "Hervé", "Ivan", "Junior",
        "Kevin", "Loïc", "Marc", "Nathan", "Olivier", "Paul", "Rodrigue", "Samuel", "Thierry", "Yannick"]
girls = ["Aïcha", "Brenda", "Carine", "Diane", "Estelle", "Flore", "Grâce", "Hélène", "Inès", "Josiane",
         "Karelle", "Laure", "Maëlle", "Nadège", "Orlane", "Patricia", "Rachel", "Sandrine", "Tatiana", "Vanessa"]
classes = [("6e A", 11), ("5e A", 12), ("4e A", 13), ("3e A", 14), ("2nde C", 15), ("Tle D", 17)]

students = []
number = 1
for class_name, age in classes:
    for _ in range(rng.randint(24, 28)):
        is_girl = rng.random() < 0.5
        surname = rng.choice(surnames)
        first = rng.choice(girls if is_girl else boys)
        year = 2026 - age
        students.append({
            "Matricule": f"DEMO26{number:04d}",
            "Nom": surname,
            "Prénom": first,
            "Sexe": "F" if is_girl else "M",
            "Date de naissance": f"{rng.randint(1, 28):02d}/{rng.randint(1, 12):02d}/{year}",
            "Lieu de naissance": "Ville Démo",
            "Classe": class_name,
            "Nom du parent": f"{rng.choice(surnames)} {rng.choice(boys + girls)}",
        })
        number += 1

with (out / "eleves-fictifs.csv").open("w", newline="", encoding="utf-8-sig") as file:
    writer = csv.DictWriter(file, fieldnames=list(students[0]), delimiter=";")
    writer.writeheader()
    writer.writerows(students)

subjects = [("Français", "PLEG"), ("Anglais", "PLEG"), ("Mathématiques", "PLEG"), ("Physique-Chimie", "PCEG"),
            ("SVT", "PCEG"), ("Histoire-Géographie", "PLEG"), ("ECM", "PCEG"), ("Informatique", "PCEG"),
            ("EPS", "PCEG"), ("Philosophie", "PLEG"), ("Espagnol", "PCEG"), ("Allemand", "PCEG")]
teachers = []
for index, (subject, grade) in enumerate(subjects, start=1):
    is_woman = index % 2 == 0
    teachers.append({
        "Nom": rng.choice(surnames),
        "Prénom": rng.choice(girls if is_woman else boys),
        "Sexe": "F" if is_woman else "M",
        "Matière": subject,
        "Grade": grade,
    })

with (out / "enseignants-fictifs.csv").open("w", newline="", encoding="utf-8-sig") as file:
    writer = csv.DictWriter(file, fieldnames=list(teachers[0]), delimiter=";")
    writer.writeheader()
    writer.writerows(teachers)

teacher_by_subject = {t["Matière"]: f'{t["Nom"]} {t["Prénom"]}' for t in teachers}
first_cycle = {"Français": 4, "Anglais": 3, "Mathématiques": 4, "Physique-Chimie": 2, "SVT": 2,
               "Histoire-Géographie": 2, "ECM": 1, "Informatique": 1, "EPS": 1}
second_cycle = {"Français": 3, "Anglais": 2, "Mathématiques": 5, "Physique-Chimie": 4, "SVT": 3,
                "Histoire-Géographie": 2, "ECM": 1, "Informatique": 2, "EPS": 1, "Espagnol": 2}
assignments = []
for class_name, _ in classes:
    if class_name in ("2nde C", "Tle D"):
        coefficients = dict(second_cycle)
        if class_name == "Tle D":
            coefficients.update({"Mathématiques": 4, "SVT": 5, "Philosophie": 3})
    else:
        coefficients = dict(first_cycle)
        if class_name in ("4e A", "3e A"):
            coefficients["Allemand"] = 2
    for subject, coefficient in coefficients.items():
        assignments.append({"Matière": subject, "Classe": class_name, "Coef": coefficient,
                            "Professeur": teacher_by_subject[subject]})

with (out / "matieres-fictives.csv").open("w", newline="", encoding="utf-8-sig") as file:
    writer = csv.DictWriter(file, fieldnames=list(assignments[0]), delimiter=";")
    writer.writeheader()
    writer.writerows(assignments)

print(len(students), "élèves,", len(teachers), "enseignants,", len(assignments), "affectations")
for class_name, _ in classes:
    print(class_name, sum(1 for s in students if s["Classe"] == class_name))
