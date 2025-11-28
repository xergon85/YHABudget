# Uppgift 1 – WPF Budgetplanerare
**Kurs: Windows applikationsutveckling**
## Beskrivning av uppdraget
Du ska utveckla en desktop-applikation i WPF som fungerar som en budgetplanerare.
Applikationen ska låta användaren registrera inkomster och utgifter, kategorisera dessa, och
visa summeringar.
Applikationen ska även kunna beräkna månadsprognoser, där användaren kan se hur nästa
månad kommer att se ut baserat på registrerade inkomster, utgifter och eventuell frånvaro.
För den som vill utmana sig själv finns en extra funktion: hantering av frånvaro
(VAB/sjukdagar) med automatiska avdrag och tillägg.

## Kravspecifikation
### Grundkrav
1. Applikationen ska byggas i WPF med MVVM-arkitektur.
2. Använd databinding för att koppla UI till ViewModel.
3. Möjlighet att:
	- Lägga till, redigera och ta bort inkomster och utgifter.
	- Varje post ska tillhöra en kategori (exempel:
	- Utgifter: Mat, Hus & drift, Transport, Fritid, Barn, Streaming-tjänster, SaaS-produkter
	- Inkomster: Lön, Bidrag, Hobbyverksamhet).
4. Registrering av utgifter ska stödja:
	- Återkommande utgifter:
	- Varje månad (ex. hyra, streamingtjänst).
	- Varje år (ex. bilförsäkring – användaren väljer månad).
	- Engångsutgifter (spontana inköp, ex. ny dator).
5. Visa summeringar och månadsprognoser:
	- Prognosen ska inkludera återkommande utgifter och inkomster.
6. Beräkna månadsinkomst utifrån:
	- Årsinkomst
	- Årsarbetstid (timmar)
7. Data ska sparas lokalt i en databas (MS SQL eller SQLite – valfritt).

1. Extra funktion (frivillig men ger högre bedömning)
1. Registrera frånvaro (datum och typ: VAB eller sjuk).
2. Automatisk beräkning:
	- Avdrag på kommande månads lön.
	- Tillägg med 80% av avdraget.
		- Om frånvaro är VAB: tak på årsinkomst (7,5 PBB = 410 000 kr).
		- Om årsinkomst > 410 000 kr, beräkna avdrag som om inkomsten var 410 000 kr.
3. Prognosen ska visa effekten av frånvaro (ex. -1000 kr för VAB, +800 kr i ersättning).
UI-krav (gäller oavsett extra funktionen eller ej)
	- Använd styles, resources och datatemplates för ett snyggt och konsekvent gränssnitt.
Tips
	- Börja med MVVM-strukturen innan du bygger UI.
	- Använd ObservableCollection för listor.
	- Testa beräkningarna separat innan du kopplar dem till UI.
	- För databas: Entity Framework är ett bra alternativ.
