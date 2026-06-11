# Magic Symbol Recognizer
Ein interaktives, KI-gestütztes Magiesystem für Windows. Spieler können mit der Maus oder dem Finger magische Runen auf ein Zeichenfeld zeichnen. Eine lokale künstliche Intelligenz erkennt die Art des Zaubers, während ein mathematischer Algorithmus die Präzision und die benötigte Zeit auswertet.

---
## Download & Demo

Du möchtest das System sofort ausprobieren? Die fertige, eigenständige Windows-Anwendung benötigt keine Installation.

*   **[Hier die spielbare Demo (.ZIP) herunterladen](https://github.com/IBimsHedebe/Magic-Symbol-Recognizer/releases/download/v1.0.0/Magic.Symbol.Recognizer.zip)**
*   *Hinweis:* Nach dem Entpacken einfach die `.exe` starten. Da die Anwendung eigenständig kompiliert wurde, ist kein installiertes .NET-Framework erforderlich (64-Bit Windows vorausgesetzt).

---
## Features

*   **Lokale KI-Erkennung:** Nutzung von Microsoft `ML.NET` zur Bildklassifizierung direkt auf dem Endgerät (vollständig offline, keine Serverkosten).
*   **Magisches Malfeld:** Ein responsives `InkCanvas` (WPF) mit visuellem Feedback (leuchtende Tinte) für den Spieler.
*   **Präzisions-Algorithmus:** Die gezeichneten Linien werden in Echtzeit mathematisch normalisiert und mit idealen Musterbeispielen (Vektorkoordinaten) verglichen.
*   **Zeitkomponente:** Schnelligkeit wird belohnt! Ein integrierter Timer misst die Zauberdauer und beeinflusst den finalen Score.
*   **Failsafe-Schwellenwert:** Zauber mit einem Gesamtergebnis von unter 40% werden als "fehlgeschlagen" oder "unerkennbar" gewertet.

---
## Technologie-Stack

*   **Sprache:** C# (.NET 8.0)
*   **UI-Framework:** WPF (Windows Presentation Foundation)
*   **KI-Framework:** ML.NET - Model Builder (Bildklassifizierung)

---
## Funktionsweise des Algorithmus

Das System kombiniert maschinelles Lernen mit klassischer Mathematik in vier Phasen:

*   **Erkennung (KI):** Beim Klick auf "Zauber wirken" analysiert das neuronale Netz das Gesamtbild und ordnet es einer Magieschule zu (z.B. *Feuer* oder *Heilung*).
*   **Bewertung mit Standart Beispielen:** Die Magie wird nun mit einem Musterbeispiel vergleicht und jenachdem, wie ähnlich sie zueinander sind, desto besser ist die Bewertung.

---
## Installation & Lokales Setup (Für Entwickler)

Wenn du den Code verändern oder eigene Symbole hinzufügen möchtest:

1. Klicke oben auf **Code -> Download ZIP** oder klone das Repository:
   ```bash
   git clone https://github.com
   ```
2. Öffne die `.sln`-Datei in **Visual Studio 2022** (oder neuer).
3. Stelle sicher, dass die Erweiterung **ML.NET Model Builder** in Visual Studio installiert ist.
4. Starte das Projekt mit `F5`.

---
##Lizenz

Dieses Projekt ist unter der MIT-Lizenz lizenziert – siehe die [LICENSE](LICENSE) Datei für Details.
