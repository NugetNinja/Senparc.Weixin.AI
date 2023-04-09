using Senparc.Weixin.AI.MPSample;
using Senparc.Weixin.AspNet;
using Senparc.Weixin.MP;
using Senparc.Weixin.MP.MessageHandlers.Middleware;
using Senparc.Weixin.RegisterServices;

var builder = WebApplication.CreateBuilder(args);


//ʹ�ñ��ػ���������
builder.Services.AddMemoryCache();

#region ���΢�����ã�һ�д��룩

//Senparc.Weixin ע�ᣨ���룩
builder.Services.AddSenparcWeixinServices(builder.Configuration);

#endregion

var app = builder.Build();

#region ����΢�����ã�һ����룩

//�ֶ���ȡ������Ϣ��ʹ�����·���
//var senparcWeixinSetting = app.Services.GetService<IOptions<SenparcWeixinSetting>>()!.Value;

//����΢�����ã����룩
var registerService = app.UseSenparcWeixin(app.Environment,
    null /* ��Ϊ null �򸲸� appsettings  �е� SenpacSetting ����*/,
    null /* ��Ϊ null �򸲸� appsettings  �е� SenpacWeixinSetting ����*/,
    register => { /* CO2NET ȫ������ */ },
    (register, weixinSetting) =>
    {
        //ע�ṫ�ں���Ϣ������ִ�ж�Σ�ע�������ںţ�
        register.RegisterMpAccount(weixinSetting, "�ҵĹ��ں�");
    });

#region ʹ�� MessageHadler �м��������ȡ������������ Controller

//MessageHandler �м�����ܣ�https://www.cnblogs.com/szw/p/Wechat-MessageHandler-Middleware.html
//ʹ�ù��ںŵ� MessageHandler �м����������Ҫ���� Controller��
app.UseMessageHandlerForMp("/WeixinAsync", CustomMessageHandler.GenerateMessageHandler, options =>
{
    options.AccountSettingFunc = context => Senparc.Weixin.Config.SenparcWeixinSetting;
});

#endregion

#endregion


app.MapGet("/", () => "��ӭʹ�� Senparc.AI + Senparc.Weixin !");

app.Run();
