# ✅ Добавлены GameObject'ы режимов для таблицы лидеров

## Дата: 2026-04-20

---

## 🎨 Что добавлено:

Теперь в панели рейтинга можно показывать разные GameObject'ы для Normal и Arcade режимов, которые автоматически включаются/выключаются при переключении.

---

## 📋 Новые поля в PanelRanking:

```csharp
[Header("Mode GameObjects")]
public GameObject normalModeObject;      // GameObject для Normal режима
public GameObject arcadeModeObject;      // GameObject для Arcade режима
```

---

## 🔧 Как настроить в Unity:

### Шаг 1: Создать GameObject'ы для режимов

1. Откройте сцену `2_Home`
2. Найдите панель рейтинга (PanelRanking)
3. Создайте два GameObject'а (например):
   - **NormalModeIcon** - для Normal режима
   - **ArcadeModeIcon** - для Arcade режима

### Шаг 2: Настроить GameObject'ы

Каждый GameObject может содержать:
- Image с иконкой режима
- Text с названием режима
- Анимацию
- Любые другие UI элементы

**Пример структуры:**
```
NormalModeIcon
├── Icon (Image) - иконка 🎯
└── Text (TextMeshPro) - "CLASSIC MODE"

ArcadeModeIcon
├── Icon (Image) - иконка ⚡
└── Text (TextMeshPro) - "ARCADE MODE"
```

### Шаг 3: Выключить GameObject'ы по умолчанию

1. Выберите **NormalModeIcon** → снимите галочку (выключите)
2. Выберите **ArcadeModeIcon** → снимите галочку (выключите)

### Шаг 4: Привязать к PanelRanking

1. Выберите GameObject с компонентом `PanelRanking`
2. В Inspector найдите секцию **Mode GameObjects**
3. Перетащите:
   - **Normal Mode Object** → NormalModeIcon
   - **Arcade Mode Object** → ArcadeModeIcon

---

## 🎮 Как работает:

### При открытии рейтинга:
- Включается GameObject текущего режима (по умолчанию Normal)
- Другой GameObject выключается

### При переключении на Normal:
- `SwitchToNormalMode()` → включает `normalModeObject`, выключает `arcadeModeObject`

### При переключении на Arcade:
- `SwitchToArcadeMode()` → включает `arcadeModeObject`, выключает `normalModeObject`

---

## 💡 Примеры дизайна GameObject'ов:

### Вариант 1: Простая иконка
```
GameObject: Image с иконкой режима
- Normal: 🎯 (мишень)
- Arcade: ⚡ (молния)
```

### Вариант 2: Иконка + Текст
```
GameObject:
├── Icon (Image)
└── ModeText (TextMeshPro) - "CLASSIC" / "ARCADE"
```

### Вариант 3: Полноценный баннер
```
GameObject:
├── Background (Image) - фон
├── Icon (Image) - иконка
├── Title (TextMeshPro) - название режима
└── Description (TextMeshPro) - описание
```

### Вариант 4: Анимированный элемент
```
GameObject с Animator:
- Анимация появления
- Пульсация
- Свечение
```

---

## 📐 Пример UI структуры:

```
PanelRanking
├── Header
│   ├── Title "RANKING"
│   ├── NormalModeIcon (выключен) ← Привязать к normalModeObject
│   ├── ArcadeModeIcon (выключен) ← Привязать к arcadeModeObject
│   └── Mode Buttons
│       ├── Button_Normal
│       └── Button_Arcade
├── MyRanking
└── RankingList
```

---

## ✅ Преимущества этого подхода:

1. **Гибкость:** Можно создать любой дизайн (иконки, текст, анимации)
2. **Простота:** Просто включаем/выключаем GameObject'ы
3. **Визуальность:** Можно настроить всё прямо в Unity Editor
4. **Анимации:** Можно добавить Animator для плавных переходов

---

## 🎨 Рекомендации:

- Разместите GameObject'ы в одном месте (они будут переключаться)
- Используйте одинаковый размер и позицию для обоих
- Добавьте анимацию появления для плавности
- Используйте яркие цвета для Arcade режима

---

## ✅ Результат:

Теперь при переключении режимов будут автоматически включаться/выключаться соответствующие GameObject'ы:
- 🎯 Normal режим → показывается normalModeObject
- ⚡ Arcade режим → показывается arcadeModeObject

Пользователи будут чётко видеть, в каком режиме они находятся!

---

**Готово к настройке в Unity!** 🎨

