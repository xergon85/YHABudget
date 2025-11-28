# UI Sketch - Budget Planner

## Main Window Layout

```
┌─────────────────────────────────────────────────────────────────┐
│  Budget Planner                                    [_] [□] [X]   │
├─────────────────────────────────────────────────────────────────┤
│                                                                   │
│  [Översikt] [Transaktioner] [Återkommande] [Inställningar]      │
│  ─────────────────────────────────────────────────────────────   │
│                                                                   │
│  Content Area (TabControl)                                       │
│                                                                   │
│                                                                   │
│                                                                   │
│                                                                   │
│                                                                   │
└─────────────────────────────────────────────────────────────────┘
```

---

## Tab 1: Översikt (Overview/Projection)

```
┌─────────────────────────────────────────────────────────────────┐
│  Månadsprognos                                                    │
│                                                                   │
│  Månad: [November 2025 ▼]                                        │
│                                                                   │
│  ┌─────────────────────────────────────────────────────────────┐ │
│  │  Inkomster                                     + 45,000 kr   │ │
│  │  ├─ Lön (beräknad)                    35,000 kr             │ │
│  │  ├─ Bidrag                             8,000 kr             │ │
│  │  └─ Hobbyverksamhet                    2,000 kr             │ │
│  │                                                              │ │
│  │  Utgifter                                      - 32,500 kr   │ │
│  │  ├─ Hus & drift                       12,000 kr             │ │
│  │  ├─ Mat                                 6,500 kr             │ │
│  │  ├─ Transport                           3,000 kr             │ │
│  │  ├─ Streaming-tjänster                   500 kr             │ │
│  │  └─ Övriga                             10,500 kr             │ │
│  │                                                              │ │
│  │  ─────────────────────────────────────────────────────────  │ │
│  │  Netto                                         + 12,500 kr   │ │
│  └─────────────────────────────────────────────────────────────┘ │
│                                                                   │
│  □ Inkludera frånvaro i beräkning                                │
│                                                                   │
└─────────────────────────────────────────────────────────────────┘
```

---

## Tab 2: Transaktioner (Transactions)

```
┌─────────────────────────────────────────────────────────────────┐
│  Transaktioner                                                    │
│                                                                   │
│  Filter: (•) Alla  ( ) Inkomster  ( ) Utgifter                  │
│  Kategori: [Alla kategorier ▼]     Månad: [Alla månader ▼]      │
│                                                                   │
│  [+ Ny transaktion]                                              │
│                                                                   │
│  ┌─────────────────────────────────────────────────────────────┐ │
│  │ Datum       │ Beskrivning      │ Kategori    │ Belopp    │ │ │
│  ├─────────────────────────────────────────────────────────────┤ │
│  │ 2025-11-01  │ Lön november     │ Lön         │ +35,000   │✏│ │
│  │ 2025-11-05  │ Hyra             │ Hus & drift │  -8,500   │✏│ │
│  │ 2025-11-12  │ ICA Maxi         │ Mat         │  -1,250   │✏│ │
│  │ 2025-11-15  │ Netflix          │ Streaming   │    -179   │✏│ │
│  │ 2025-11-18  │ Ny laptop        │ Teknik      │ -15,000   │✏│ │
│  │ 2025-11-20  │ Bensin           │ Transport   │    -850   │✏│ │
│  │ ...                                                          │ │
│  └─────────────────────────────────────────────────────────────┘ │
│                                                                   │
│  Total: -10,779 kr                                               │
│                                                                   │
└─────────────────────────────────────────────────────────────────┘
```

---

## Tab 3: Återkommande (Recurring Transactions)

```
┌─────────────────────────────────────────────────────────────────┐
│  Återkommande transaktioner                                       │
│                                                                   │
│  [+ Ny återkommande transaktion]                                 │
│                                                                   │
│  ┌─────────────────────────────────────────────────────────────┐ │
│  │ Typ      │ Beskrivning    │ Kategori    │ Belopp   │ Aktiv│ │
│  ├─────────────────────────────────────────────────────────────┤ │
│  │ Månad    │ Hyra           │ Hus & drift │  -8,500  │  ☑   │✏│ │
│  │ Månad    │ Lön            │ Lön         │ +35,000  │  ☑   │✏│ │
│  │ Månad    │ Netflix        │ Streaming   │    -179  │  ☑   │✏│ │
│  │ Månad    │ Spotify        │ Streaming   │    -119  │  ☑   │✏│ │
│  │ År (Jan) │ Bilförsäkring  │ Försäkring  │  -8,900  │  ☑   │✏│ │
│  │ År (Jul) │ Semester       │ Fritid      │ -25,000  │  ☑   │✏│ │
│  │ Månad    │ Gymkort        │ Fritid      │    -399  │  ☐   │✏│ │
│  │ ...                                                          │ │
│  └─────────────────────────────────────────────────────────────┘ │
│                                                                   │
└─────────────────────────────────────────────────────────────────┘
```

---

## Tab 4: Inställningar (Settings)

```
┌─────────────────────────────────────────────────────────────────┐
│  Inställningar                                                    │
│                                                                   │
│  ┌─ Löneberäkning ──────────────────────────────────────────────┐│
│  │                                                               ││
│  │  Årsinkomst (brutto):  [410000] kr                           ││
│  │  Årsarbetstid:         [1920] timmar                         ││
│  │                                                               ││
│  │  → Beräknad månadslön: 17,708 kr (före skatt)                ││
│  │                                                               ││
│  └───────────────────────────────────────────────────────────────┘│
│                                                                   │
│  ┌─ Kategorier ──────────────────────────────────────────────────┐│
│  │                                                               ││
│  │  Utgifter:                     Inkomster:                    ││
│  │  • Mat                         • Lön                         ││
│  │  • Hus & drift                 • Bidrag                      ││
│  │  • Transport                   • Hobbyverksamhet             ││
│  │  • Fritid                                                    ││
│  │  • Barn                        [+ Lägg till kategori]        ││
│  │  • Streaming-tjänster                                        ││
│  │  • SaaS-produkter                                            ││
│  │                                                               ││
│  │  [+ Lägg till kategori]                                      ││
│  │                                                               ││
│  └───────────────────────────────────────────────────────────────┘│
│                                                                   │
│  ┌─ Frånvaro (Extra funktion) ───────────────────────────────────┐│
│  │                                                               ││
│  │  [+ Registrera frånvaro]                                     ││
│  │                                                               ││
│  │  Datum       │ Typ  │ Avdrag   │ Ersättning (80%)            ││
│  │  ────────────────────────────────────────────────────────────││
│  │  2025-11-10  │ VAB  │ -1,000kr │ +800kr                   │✏││
│  │  2025-11-22  │ Sjuk │ -1,000kr │ +800kr                   │✏││
│  │                                                               ││
│  └───────────────────────────────────────────────────────────────┘│
│                                                                   │
└─────────────────────────────────────────────────────────────────┘
```

---

## Dialog: Add/Edit Transaction

```
┌─────────────────────────────────────────┐
│  Ny transaktion                [X]      │
├─────────────────────────────────────────┤
│                                         │
│  Typ:        (•) Utgift  ( ) Inkomst   │
│                                         │
│  Beskrivning: [________________]        │
│                                         │
│  Belopp:      [________] kr            │
│                                         │
│  Kategori:    [Mat           ▼]        │
│                                         │
│  Datum:       [2025-11-26    📅]       │
│                                         │
│                                         │
│         [Avbryt]        [Spara]        │
│                                         │
└─────────────────────────────────────────┘
```

---

## Dialog: Add/Edit Recurring Transaction

```
┌─────────────────────────────────────────┐
│  Ny återkommande transaktion   [X]      │
├─────────────────────────────────────────┤
│                                         │
│  Typ:        (•) Utgift  ( ) Inkomst   │
│                                         │
│  Beskrivning: [________________]        │
│                                         │
│  Belopp:      [________] kr            │
│                                         │
│  Kategori:    [Hus & drift   ▼]        │
│                                         │
│  Återkommer:  (•) Varje månad          │
│               ( ) Varje år (månad: [▼])│
│                                         │
│  Startdatum:  [2025-11-01    📅]       │
│                                         │
│  ☑ Aktiv                                │
│                                         │
│         [Avbryt]        [Spara]        │
│                                         │
└─────────────────────────────────────────┘
```

---

## Color Scheme Suggestion

- **Income**: Green (#4CAF50) for positive numbers
- **Expense**: Red (#F44336) for negative numbers
- **Neutral**: Gray (#757575) for labels
- **Primary**: Blue (#2196F3) for buttons and accents
- **Background**: White (#FFFFFF) or Light Gray (#F5F5F5)
