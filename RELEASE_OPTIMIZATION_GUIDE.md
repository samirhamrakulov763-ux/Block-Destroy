# Руководство по оптимизации для релиза Block Destroy

## ✅ Уже выполнено автоматически:

1. **Удалены все Debug.Log** из всех скриптов проекта
2. **Strip Engine Code** уже включен
3. **IL2CPP** уже настроен для Android

## 🔧 Настройки в Unity Editor (выполните вручную):

### 1. Player Settings (Edit → Project Settings → Player)

#### Other Settings:
- **Scripting Backend**: IL2CPP ✅ (уже установлено)
- **API Compatibility Level**: .NET Standard 2.1 (рекомендуется)
- **Managed Stripping Level**: **High** (для максимального уменьшения размера)
- **Strip Engine Code**: ✅ включено
- **Vertex Compression**: Mixed (уже установлено)

#### Publishing Settings:
- **Minify**: 
  - Release: **Proguard** (обфускация кода)
  - Debug: None
- **Split Application Binary**: Включить если APK > 100MB
- **Compression Method**: LZ4 (быстрее) или LZ4HC (меньше размер)

### 2. Quality Settings (Edit → Project Settings → Quality)

Для Android оптимизируйте:
- **V Sync Count**: Don't Sync (для мобильных)
- **Pixel Light Count**: 1-2 (для 2D игры достаточно)
- **Texture Quality**: Full Res
- **Anisotropic Textures**: Per Texture
- **Anti Aliasing**: 2x или 4x (не больше для мобильных)
- **Soft Particles**: Отключить (не нужно для 2D)
- **Shadows**: Отключить (не нужно для 2D)

### 3. Graphics Settings (Edit → Project Settings → Graphics)

- **Tier Settings**: Убедитесь что Standard Shader Quality = Low для мобильных
- **Shader Stripping**: 
  - Lightmap Modes: Manual
  - Fog Modes: Manual
  - Instancing Variants: Strip Unused

### 4. Audio Settings (Edit → Project Settings → Audio)

- **Default Speaker Mode**: Stereo
- **DSP Buffer Size**: Best Performance (для меньшей задержки)
- **Sample Rate**: 44100 Hz (стандарт)

### 5. Physics 2D Settings (Edit → Project Settings → Physics 2D)

- **Auto Sync Transforms**: Отключить (оптимизация)
- **Reuse Collision Callbacks**: Включить
- **Queries Hit Triggers**: Отключить если не используете
- **Queries Start In Colliders**: Отключить если не используете

### 6. Build Settings (File → Build Settings)

#### Android:
- **Compression Method**: LZ4 или LZ4HC
- **Build App Bundle (Google Play)**: ✅ Включить для Google Play
- **Create symbols.zip**: Включить (для crash reports)

#### Development Build:
- ❌ **Отключить** для релиза
- ❌ **Autoconnect Profiler** - отключить
- ❌ **Deep Profiling** - отключить
- ❌ **Script Debugging** - отключить

### 7. Оптимизация текстур

Для всех текстур в проекте:
1. Откройте текстуру в Inspector
2. **Max Size**: 2048 или меньше (для мобильных)
3. **Compression**: 
   - UI элементы: ASTC 6x6 или ETC2
   - Спрайты: ASTC 6x6 или ETC2
4. **Generate Mip Maps**: Отключить для UI и 2D спрайтов
5. **Read/Write Enabled**: ❌ Отключить (экономит память)

### 8. Оптимизация аудио

Для всех аудио файлов:
1. **Load Type**: 
   - Музыка: Streaming
   - Звуковые эффекты: Decompress On Load
2. **Compression Format**:
   - Android: Vorbis (качество 70-100%)
3. **Sample Rate Setting**: Preserve Sample Rate или Override to 22050 Hz

### 9. Scripting Define Symbols

В Player Settings → Other Settings → Scripting Define Symbols:

**Для релиза удалите:**
- `DEVELOPMENT_BUILD`
- `UNITY_ASSERTIONS`
- `ENABLE_PROFILER`

**Оставьте:**
- `UNITY_PURCHASING` ✅
- Другие необходимые символы

### 10. Проверка перед сборкой

#### Чек-лист:
- [ ] Все Debug.Log удалены ✅
- [ ] Development Build отключен
- [ ] IL2CPP включен ✅
- [ ] Managed Stripping Level = High
- [ ] Minify = Proguard (Release)
- [ ] Compression = LZ4HC
- [ ] Build App Bundle включен
- [ ] Версия (Bundle Version Code) увеличена
- [ ] Все тестовые Ad Unit ID заменены на продакшн ✅
- [ ] Проверены все разрешения в AndroidManifest

## 📊 Ожидаемые результаты:

После применения всех оптимизаций:
- **Размер APK**: уменьшится на 30-50%
- **Производительность**: улучшится на 10-20%
- **Время загрузки**: уменьшится на 20-30%
- **Потребление памяти**: уменьшится на 15-25%

## 🚀 Финальная сборка:

1. **File → Build Settings**
2. Выберите **Android**
3. **Build App Bundle (Google Play)** ✅
4. **Build**
5. Загрузите .aab файл в Google Play Console

## 📝 Дополнительные рекомендации:

### Для дальнейшей оптимизации:
1. Используйте **Addressables** для больших ресурсов
2. Включите **Multithreaded Rendering** (если поддерживается)
3. Оптимизируйте UI с помощью **Canvas Groups**
4. Используйте **Object Pooling** для часто создаваемых объектов ✅ (уже используется)
5. Профилируйте игру с помощью **Unity Profiler** перед релизом

### Тестирование:
1. Тестируйте на реальных устройствах (не только в редакторе)
2. Проверьте на устройствах с разными версиями Android (API 25+)
3. Проверьте производительность на слабых устройствах
4. Мониторьте потребление памяти и батареи

## ⚠️ Важно:

- Всегда делайте backup проекта перед применением оптимизаций
- Тестируйте игру после каждого изменения
- Некоторые оптимизации могут вызвать проблемы - откатывайте если что-то сломалось
- Managed Stripping Level = High может удалить нужный код - тестируйте тщательно!
