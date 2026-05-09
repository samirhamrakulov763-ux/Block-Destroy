# Инструкция по замене Spine анимации на Unity UI анимацию

## Шаг 1: Подготовка шрифта

1. Найдите похожий шрифт на оригинальный (жирный, игровой стиль)
2. Импортируйте шрифт в проект или используйте существующий
3. Создайте TextMeshPro Font Asset (Window → TextMeshPro → Font Asset Creator)

## Шаг 2: Создание UI структуры на сцене 1_Title

Откройте сцену `1_Title.unity` и создайте следующую структуру:

```
Canvas
└── TitleAnimation (GameObject с компонентом TitleTextAnimation)
    ├── TopRow (Empty GameObject)
    │   ├── Letter_B (TextMeshProUGUI)
    │   ├── Letter_L (TextMeshProUGUI)
    │   ├── Letter_O (TextMeshProUGUI)
    │   ├── Letter_C (TextMeshProUGUI)
    │   └── Letter_K (TextMeshProUGUI)
    ├── BottomRow (Empty GameObject)
    │   ├── Letter_D (TextMeshProUGUI)
    │   ├── Letter_E (TextMeshProUGUI)
    │   ├── Letter_S (TextMeshProUGUI)
    │   ├── Letter_T (TextMeshProUGUI)
    │   ├── Letter_R (TextMeshProUGUI)
    │   ├── Letter_O (TextMeshProUGUI)
    │   └── Letter_Y (TextMeshProUGUI)
    └── BackgroundGlow (Image с CanvasGroup)
```

## Шаг 3: Настройка букв

Для каждой буквы (TextMeshProUGUI):
1. Установите текст (B, L, O, C, K для верхнего ряда)
2. Установите Font Size: ~120-150 (подберите под размер экрана)
3. Установите цвет: белый (#FFFFFF)
4. Добавьте Outline (Material Preset или через Material):
   - Outline Color: темно-синий/черный
   - Outline Width: 0.2-0.3
5. Расположите буквы в ряд с небольшим отступом (~70-80 пикселей между буквами)

### Позиции (примерные):
**TopRow** (Y: 288):
- B: X: -220
- L: X: -150
- O: X: -75
- C: X: -5
- K: X: 70

**BottomRow** (Y: 175):
- D: X: -240
- E: X: -170
- S: X: -100
- T: X: -30
- R: X: 40
- O: X: 110
- Y: X: 180

## Шаг 4: Настройка фона (BackgroundGlow)

1. Создайте Image компонент
2. Добавьте CanvasGroup компонент
3. Установите цвет: голубой полупрозрачный (#00BFFF80)
4. Растяните на весь экран или под буквы
5. Установите за буквами (Order in Layer)

## Шаг 5: Настройка компонента TitleTextAnimation

1. Выберите GameObject `TitleAnimation`
2. Добавьте компонент `TitleTextAnimation`
3. Перетащите все буквы верхнего ряда в массив `Top Letters` (5 элементов)
4. Перетащите все буквы нижнего ряда в массив `Bottom Letters` (7 элементов)
5. Перетащите BackgroundGlow в поле `Background Glow`

## Шаг 6: Отключение старой Spine анимации

1. Найдите GameObject `SkeletonGraphic (Title)` на сцене
2. Отключите его (снимите галочку Active) или удалите

## Шаг 7: Тестирование

1. Запустите сцену 1_Title
2. Буквы должны появляться по очереди с bounce эффектом
3. В конце должен мигнуть синий фон

## Настройка параметров анимации (опционально)

В компоненте TitleTextAnimation можно настроить:
- `Letter Delay`: задержка между буквами (по умолчанию 0.0667 сек)
- `Bounce Duration`: длительность bounce эффекта (0.3 сек)
- `Bounce Height`: высота подпрыгивания (5.42)
- `Bounce Scale`: масштаб при bounce (1.119, 1.066, 1)
- `Glow Start Time`: когда начинается мигание фона (3.1 сек)
- `Glow Duration`: длительность мигания (0.3 сек)

## Дополнительные улучшения (опционально)

### Добавить тень:
1. Дублируйте каждую букву
2. Сделайте копию черной и немного сдвиньте вниз-вправо
3. Поместите за оригинальной буквой

### Добавить градиент:
1. Используйте Vertex Color в TextMeshPro
2. Или создайте Material с градиентом

### Добавить частицы:
1. Добавьте Particle System при появлении каждой буквы
2. Искры, звездочки и т.д.
