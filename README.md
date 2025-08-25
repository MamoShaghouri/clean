# README

## مشروع نظام الأشكال الاحترافي (Professional Shape System)

هذا المشروع هو تطبيق احترافي لإدارة وتحرير الأشكال الهندسية، مبني على بنية معمارية متقدمة ومطابقة لمعايير التطوير الحديثة.

## البنية المعمارية (Architecture)

### المخطط العام للنظام

@startuml
' Core Shape System
abstract class Shape {
    +string Name
    +Arrow GrainLine
    +List<TextLabel> TextPoints
    +ShapeGeometry Geometry
    +Dictionary<string, ShapeSizeData> Sizes
    +bool IsSelected

    +abstract void Draw(Graphics g)
    +abstract bool HitTest(PointF point)
    +abstract void Move(PointF delta)
    +abstract void Resize(ResizeHandle handle, PointF newPosition)
    +abstract void Rotate(float angle)
    +virtual Shape Clone()
}

class RectangleShape {
    +RectangleF Bounds
    +Draw(Graphics g)
    +HitTest(PointF point)
    +Move(PointF delta)
    +Resize(ResizeHandle handle, PointF pos)
    +Rotate(float angle)
}

class CircleShape {
    +PointF Center
    +float Radius
    +Draw(Graphics g)
    +HitTest(PointF point)
    +Move(PointF delta)
    +Resize(ResizeHandle handle, PointF pos)
    +Rotate(float angle)
}

abstract class ShapeGeometry {
    +abstract void Translate(PointF delta)
    +abstract bool Contains(PointF point)
}

class RectangleGeometry
class CircleGeometry

class Arrow {
    +PointF Start
    +PointF End
    +void Draw(Graphics g)
}

class TextLabel {
    +string Text
    +PointF Position
    +Font Font
    +void Draw(Graphics g)
}

class ShapeSizeData {
    +string SizeName
    +ShapeGeometry Geometry
}

class ShapeRenderer {
    +void RenderShapes(Graphics g, IEnumerable<Shape> shapes)
}

' Editor / Tooling
class ShapeEditor {
    -Shape selectedShape
    +Move(PointF delta)
    +Resize(ResizeHandle handle, PointF position)
    +Rotate(float angle)
    +ModifySizeLayer(string size, Action<ShapeSizeData> modification)
}

enum ResizeHandle

' Inheritance
Shape <|-- RectangleShape
Shape <|-- CircleShape
ShapeGeometry <|-- RectangleGeometry
ShapeGeometry <|-- CircleGeometry

' Associations
Shape --> Arrow : has
Shape --> TextLabel : contains
Shape --> ShapeGeometry : uses
Shape --> "Dictionary<string,ShapeSizeData>" : sizes
ShapeSizeData --> ShapeGeometry
ShapeRenderer --> Shape : renders
ShapeEditor --> Shape : edits

@enduml

## الميزات الرئيسية (Key Features)

### 🎯 **نظام الأشكال المتقدم**
- **فئة أساسية مجردة**: `Shape` مع عمليات مشتركة
- **أشكال متعددة**: `RectangleShape` و `CircleShape`
- **هندسة مرنة**: `ShapeGeometry` مع تطبيقات مختلفة

### 🔧 **أدوات التحرير المتقدمة**
- **محرر مركزي**: `ShapeEditor` لإدارة جميع العمليات
- **مقابض تغيير الحجم**: `ResizeHandle` مع 10 أنواع مختلفة
- **نظام النقاط**: `AnchorPoint` للتحكم الدقيق

### 📏 **إدارة الأحجام المتقدمة**
- **أحجام متعددة**: `ShapeSizeData` لكل شكل
- **توسيع مرن**: إمكانية إضافة أحجام جديدة
- **خصائص مخصصة**: `CustomProperties` لكل حجم

### 🎨 **نظام العرض المتقدم**
- **عرض متعدد**: `ShapeRenderer` للعرض الاحترافي
- **أسهم الاتجاه**: `Arrow` لخطوط الحبوب
- **نصوص مرنة**: `TextLabel` مع تنسيق متقدم

## البنية التقنية (Technical Structure)

### 📁 **الملفات الأساسية**
```
├── Shape.cs                 # الفئة الأساسية للأشكال
├── RectangleShape.cs        # تطبيق المستطيل
├── CircleShape.cs           # تطبيق الدائرة
├── ShapeGeometry.cs         # الهندسة المجردة
├── RectangleGeometry.cs     # هندسة المستطيل
├── CircleGeometry.cs        # هندسة الدائرة
├── Arrow.cs                 # سهم الاتجاه
├── TextLabel.cs             # النصوص والملصقات
├── ShapeSizeData.cs         # بيانات الأحجام
├── ResizeHandle.cs          # مقابض التغيير
├── ShapeRenderer.cs         # نظام العرض
└── ShapeEditor.cs           # محرر الأشكال
```

### 🏗️ **المبادئ المعمارية**
- **SOLID Principles**: تطبيق مبادئ التصميم الصلب
- **Dependency Injection**: حقن التبعيات
- **Factory Pattern**: نمط المصنع للأشكال
- **Observer Pattern**: نمط المراقب للأحداث
- **Command Pattern**: نمط الأمر للعمليات

### 🔄 **التوافقية**
- **System.Drawing**: للرسم الأساسي
- **WPF Compatibility**: توافق مع واجهة WPF
- **Cross-Platform**: قابل للتشغيل على منصات متعددة

## كيفية الاستخدام (Usage)

### إنشاء شكل جديد
```csharp
// إنشاء مستطيل
var rectangle = new RectangleShape(100, 100, 200, 150);
rectangle.Name = "مستطيل رئيسي";

// إنشاء دائرة
var circle = new CircleShape(new PointF(300, 200), 75);
circle.Name = "دائرة مساعدة";
```

### تحرير الشكل
```csharp
var editor = new ShapeEditor();
editor.AddShape(rectangle);
editor.SelectedShape = rectangle;

// تحريك الشكل
editor.Move(new PointF(50, 30));

// تغيير الحجم
editor.Resize(ResizeHandle.BottomRight, new PointF(250, 200));

// دوران الشكل
editor.Rotate(45.0f);
```

### عرض الأشكال
```csharp
var renderer = new ShapeRenderer();
using (var graphics = CreateGraphics())
{
    renderer.RenderShapes(graphics, editor.Shapes);
}
```

## المتطلبات (Requirements)

### 📋 **متطلبات النظام**
- **.NET Framework 4.7.2** أو أحدث
- **Windows 10/11** أو أحدث
- **Visual Studio 2019/2022** للتطوير

### 📦 **الحزم المطلوبة**
- **System.Drawing.Common**: للرسم
- **System.Windows.Forms**: للواجهة
- **System.Windows.Presentation**: لـ WPF

## التطوير المستقبلي (Future Development)

### 🚀 **الميزات المخططة**
- [ ] **أشكال متقدمة**: منحنى بيزير، مضلعات
- [ ] **طبقات متعددة**: نظام طبقات متقدم
- [ ] **تصدير متعدد**: PDF, SVG, DXF
- [ ] **قاعدة بيانات**: حفظ واسترجاع المشاريع
- [ ] **تعاون متعدد**: دعم العمل الجماعي

### 🔧 **التحسينات التقنية**
- [ ] **OpenGL Rendering**: عرض ثلاثي الأبعاد
- [ ] **GPU Acceleration**: تسريع الرسومات
- [ ] **Plugin System**: نظام الإضافات
- [ ] **API REST**: واجهة برمجة خارجية

## المساهمة (Contributing)

نرحب بمساهماتكم! يرجى اتباع الخطوات التالية:

1. **Fork** المشروع
2. **Create** فرع للميزة الجديدة
3. **Commit** التغييرات
4. **Push** إلى الفرع
5. **Create** طلب دمج

## الترخيص (License)

هذا المشروع مرخص تحت رخصة **MIT** - راجع ملف `LICENSE` للتفاصيل.

## الدعم (Support)

للمساعدة والدعم:
- 📧 **البريد الإلكتروني**: support@shapesystem.com
- 💬 **الدردشة**: Discord Server
- 📖 **الوثائق**: Wiki Documentation
- 🐛 **الأخطاء**: GitHub Issues

---

**تم تطوير هذا المشروع باحترافية عالية ومطابقة لأحدث معايير التطوير** 🎯✨
