# Шрифты ГОСТ

Поместите сюда файлы шрифтов:
- `GostA.ttf` — шрифт ГОСТ типа А
- `GostB.ttf` — шрифт ГОСТ типа Б

Эти шрифты встраиваются в приложение как ресурсы (Resource) и доступны через:
```
FontFamily="pack://application:,,,/Resources/Fonts/#GOST Type AU"
FontFamily="pack://application:,,,/Resources/Fonts/#GOST Type BU"
```

**Важно:** внутреннее имя шрифта (после `#`) чувствительно к регистру.
- `GostA.ttf` → `#GOST Type AU`
- `GostB.ttf` → `#GOST Type BU`

Проверить внутреннее имя можно через `GlyphTypeface.FamilyNames`.

**Где взять шрифты:**
Шрифты ГОСТ распространяются свободно и могут быть найдены на специализированных ресурсах
по оформлению технической документации по ЕСКД.
