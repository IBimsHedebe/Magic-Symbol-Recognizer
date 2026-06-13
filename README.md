# Magic Symbol Recognizer
Ein interaktives, KI-gestütztes Magiesystem für Windows. Spieler können mit der Maus oder dem Finger magische Runen auf ein Zeichenfeld zeichnen. Eine lokale künstliche Intelligenz erkennt die Art des Zaubers, während ein mathematischer Punkte-Resampling-Algorithmus die Präzision und die benötigte Zeit auswertet.

---
## Download & Demo

Du möchtest das System sofort ausprobieren? Die fertige, eigenständige Windows-Anwendung benötigt keine Installation.

*   **[Hier die spielbare Demo (.ZIP) herunterladen](https://github.com/IBimsHedebe/Magic-Symbol-Recognizer/releases/download/v1.0.0/Magic.Symbol.Recognizer.zip)**
*   *Hinweis:* Nach dem Entpacken einfach die `.exe` starten. Da die Anwendung eigenständig kompiliert wurde, ist kein installiertes .NET-Framework erforderlich (64-Bit Windows vorausgesetzt).

---
## Features

*   **Zentrales Hauptmenü:** Nahtlose Steuerung über drei Hauptoptionen (*Zaubern*, *Bibliothek*, *Schließen*) mitsamt universeller Zurück-Navigation in allen Untermenüs.
*   **Lokale KI-Erkennung:** Nutzung von Microsoft `ML.NET` zur Bildklassifizierung direkt auf dem Endgerät (vollständig offline, keine Serverkosten oder Latenzen).
*   **Unverzerrte Quadrate:** Dynamische Benutzeroberfläche, die das Malfeld und die Vorschau bei jeder Fenstergröße in einem perfekten 1:1-Verhältnis (Quadrat) hält, damit Kreise und Symbole nicht verzerren.
*   **Lebendige Bibliothek:** Ein integriertes Buch der Runen, das alle bekannten Zauber auflistet und die perfekten Musterlösungen live aus den internen Koordinaten auf den Bildschirm zeichnet.
*   **Manipulationssicher:** Die perfekten Musterlösungen sind als verschlüsselte JSON-Ressourcen ("Embedded Resources") direkt in die `.exe`-Datei hineingeschmolzen.
*   **Vorlagen-Editor:** Neue Zauberrunen können direkt im laufenden Programm als Entwickler-Funktion gezeichnet und als `.json`-Koordinaten exportiert werden.
*   **Ghosting:** Beim Übungsbereich kann man, wenn die Magie unter 70% ist eine rote linie als musterbeispiel zum übern haben.

---
## Modus
*   **Übungsmodus:** lerne wie du die Magie malst
*   **Battle Arena:** bekämpfe eine AI und versuch sie zu besiegen
*   **Bibliothek:** Schaue nach, was für magie du malen kannst 

---
## Die 7 Magie Arten

Das System ist auf den exakten, sequenziellen Linienabgleich von sieben elementaren Ur-Runen trainiert:
*   **Dark:** Ein kreisrunder Pfad, gefolgt von einem flüssig angeschlossenen, diagonalen Kreuz (X) in der Mitte.
*   **Earth:** Ein mathematisch exaktes, im Uhrzeigersinn geschlossenes Quadrat.
*   **Fire:** Ein stabiles, gleichschenkliges Dreieck, beginnend an der oberen Spitze.
*   **Light:** Ein hochauflösender, runder Kreis aus 12 interpolierten Konturpunkten.
*   **Thunder:** Das klassische, kantige Strom-Blitz-Symbol mit scharfen Richtungswechseln.
*   **Water:** Eine flüssige, mathematisch harmonische Sinus-Welle (Tilde-Zeichen).
*   **Wind:** Drei vertikale, parallele Striche mit identischem Abstand, optimiert als durchgängige S-Linie.

---
## Technologie-Stack

*   **Sprache:** C# (.NET 8.0)
*   **UI-Framework:** WPF (Windows Presentation Foundation)
*   **KI-Framework:** ML.NET - Model Builder (Bildklassifizierung)
*   **Datenformat:** JSON (für die mathematischen Vektor-Muster)

---
## Funktionsweise des Algorithmus

Das System kombiniert maschinelles Lernen mit klassischer Mathematik in vier Phasen:

1.  **Erkennung (KI):** Beim Klick auf "Zauber wirken" analysiert das neuronale Netz (`ML.NET`) das Gesamtbild und ordnet es einer Magieschule zu.
2.  **Punkte-Normalisierung:** Unabhängig davon, ob der Spieler die Rune winzig in die Ecke oder riesig in die Mitte zeichnet, skaliert der Algorithmus die Koordinaten auf eine Standard-Einheitsbox von `0.0` bis `1.0`.
3.  **Zeitgesteuertes Resampling (Punkte-Stretching):** Da Spieler Hunderte von Punkten erzeugen, die Vorlagen jedoch nur aus prägnanten Eckpunkten bestehen, berechnet das System den prozentualen Zeitfortschritt. Es pickt die exakt zueinander passenden Linienabschnitte heraus, um die Linien synchron übereinanderzulegen.
4.  **Geschwindigkeits-Faktor:** Die Stoppuhr misst die Dauer des Zaubers. Dem Spieler stehen 4 Sekunden freie Konzentrationszeit zur Verfügung – jede Sekunde darüber führt zu einem milden Punkteabzug.
5.  **Schwellenwert-Logik:** Form-Genauigkeit (90%) und Zeit (10%) ergeben den Gesamtscore. Liegt dieser **unter 40%**, verpufft die Magie wirkungslos und wird als *unerkenntlich* gewertet.

---
### Problem

*   **Magie wird nicht erkannt:** Die Magie Wind und Thunder wird im moment nicht erkant und daher nicht wirklich benutzt werden können.

---
## Installation & Lokales Setup (Für Entwickler)

Wenn du den Code verändern oder eigene Symbole hinzufügen möchtest:

1. Klicke oben auf **Code -> Download ZIP** oder klone das Repository:
   ```bash
   git clone https://github.com/IBimsHedebe/Magic-Symbol-Recognizer
   ```
2. Öffne die `.sln`-Datei in **Visual Studio 2022** (oder neuer).
3. Stelle sicher, dass die Erweiterung **ML.NET Model Builder** in Visual Studio installiert ist.
4. Starte das Projekt mit `F5`.

---
## Lizenz

Dieses Projekt ist unter der MIT-Lizenz lizenziert – siehe die [LICENSE](LICENSE) Datei für Details.
