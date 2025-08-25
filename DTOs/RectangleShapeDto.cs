using System.Collections.Generic;

namespace Shaghouri.DTOs
{
    /// <summary>
    /// نموذج نقل بيانات المستطيل
    /// </summary>
    public class RectangleShapeDto
    {
        /// <summary>
        /// الموقع الأفقي
        /// </summary>
        public double X { get; set; }
        
        /// <summary>
        /// الموقع العمودي
        /// </summary>
        public double Y { get; set; }
        
        /// <summary>
        /// العرض
        /// </summary>
        public double Width { get; set; }
        
        /// <summary>
        /// الارتفاع
        /// </summary>
        public double Height { get; set; }
        
        /// <summary>
        /// اسم المستطيل
        /// </summary>
        public string Name { get; set; }
        
        /// <summary>
        /// زاوية خط الحبوب
        /// </summary>
        public double ArrowAngle { get; set; }
        
        /// <summary>
        /// طول خط الحبوب
        /// </summary>
        public double ArrowLength { get; set; }
        
        /// <summary>
        /// النقاط المرجعية
        /// </summary>
        public List<AnchorPointDto> AnchorPoints { get; set; } = new List<AnchorPointDto>();
    }
} 