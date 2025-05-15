namespace CaptchaApi.Models;

public class CaptchaData
{
    public float MaxSpeed { get; set; }               // ✅ أعلى سرعة لحركة الماوس (تدل على النشاط المفاجئ)
    public float LastSpeed { get; set; }              // ✅ آخر سرعة مسجلة قبل النقر (تفيد بحالة التباطؤ)
    public float SpeedStability { get; set; }         // ✅ مدى استقرار الحركة (هل الماوس يتحرك بثبات أم عشوائية؟)
    public int MovementTime { get; set; }             // ✅ الزمن الكلي بين بداية الحركة والنقر (بالميلي ثانية)
    public List<float>? SpeedSeries { get; set; }     // ✅ سلسلة سرعات الماوس (لقطات متعددة تمثل السلوك العام)

    public string? AttemptId { get; set; }            // ✅ معرف المحاولة (مفيد للتتبع أو الربط بين البيانات)
    public string? UserAgent { get; set; }            // ✅ بيانات المتصفح والجهاز (نوع المتصفح، النظام...)
    public string? BehaviorType { get; set; }         // ✅ النتيجة النهائية للسلوك: human / robot / uncertain

    public string? Status { get; set; }               // ✅ هل تم قبول المحاولة أو تم حظرها؟ (accepted/banned)
    public string? PageUrl { get; set; }              // ✅ عنوان الصفحة التي أُجريت منها المحاولة
    public string? Reason { get; set; }               // ✅ السبب النصي في حالة الرفض أو الحظر (مثل: "too fast")
    public float DecelerationRate { get; set; }    // ✅ معدل التباطؤ لحركة الماوس








    //  public float DecelerationRate { get; set; }         // ✅ معدل التباطؤ أثناء الحركة
    //  public int DecelerationRatio { get; set; }          // ✅ نسبة التباطؤ

    // public string? SpeedTrend { get; set; }             // ✅ نمط تغير السرعة (nullable)
    //  public string? MovementPattern { get; set; }        // ✅ نمط حركة الماوس (nullable)

    // ✅ إضافات لدعم تحليل النقرات المشبوهة والروبوتات
    //  public bool? ClickedFakeBox { get; set; }           // ✅ هل النقر كان على مربع وهمي؟
    //  public int? ErrorCode { get; set; }                 // ✅ كود الخطأ الموحد المرتبط بالنقر
    //  public int? BoxIndex { get; set; }                  // ✅ رقم المربع الذي تم النقر عليه

    // ✅ دعم ربط المحاولات عبر الجلسات



}
