using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Senparc.AI.Interfaces;
using Senparc.AI.Kernel.Tests.MockEntities;
using Senparc.CO2NET;
using Senparc.CO2NET.RegisterServices;

namespace Senparc.AI.Tests
{
    //[TestClass]
    public class BaseTest
    {
        public static IServiceProvider serviceProvider;
        protected static IRegisterService registerService;
        protected static SenparcSetting _senparcSetting;
        protected static ISenparcAiSetting _senparcAiSetting;


        public BaseTest(Action<IRegisterService> registerAction, Func<IConfigurationRoot, ISenparcAiSetting> senparcAiSettingFunc)
        {
            RegisterServiceCollection(senparcAiSettingFunc);

            RegisterServiceStart(registerAction);
        }

        public BaseTest() : this(null, null)
        {

        }

        /// <summary>
        /// ע�� IServiceCollection �� MemoryCache
        /// </summary>
        public static void RegisterServiceCollection(Func<IConfigurationRoot, ISenparcAiSetting> senparcAiSettingFunc)
        {
            var serviceCollection = new ServiceCollection();

            var configBuilder = new ConfigurationBuilder();

            var testFile = Path.Combine(Senparc.CO2NET.Utilities.ServerUtility.AppDomainAppPath, "appsettings.test.json");
            var appsettingFileName = File.Exists(testFile) ? "appsettings.test.json" : "appsettings.json";

            configBuilder.AddJsonFile(appsettingFileName, false, false);
            var config = configBuilder.Build();
            serviceCollection.AddSenparcGlobalServices(config);

            _senparcSetting = new SenparcSetting() { IsDebug = true };
            config.GetSection("SenparcSetting").Bind(_senparcSetting);

            _senparcAiSetting ??= senparcAiSettingFunc?.Invoke(config) 
                                   ?? new MockSenparcAiSetting() { IsDebug = true };
            config.GetSection("SenparcAiSetting").Bind(_senparcAiSetting);

            serviceCollection.AddMemoryCache();//ʹ���ڴ滺��

            serviceProvider = serviceCollection.BuildServiceProvider();
        }

        /// <summary>
        /// ע�� RegisterService.Start()
        /// </summary>
        public static void RegisterServiceStart(Action<IRegisterService> registerAction, bool autoScanExtensionCacheStrategies = false)
        {
            //ע��
            var mockEnv = new Mock<Microsoft.Extensions.Hosting.IHostEnvironment/*IHostingEnvironment*/>();
            mockEnv.Setup(z => z.ContentRootPath).Returns(() => UnitTestHelper.RootPath);

            registerService = Senparc.CO2NET.AspNet.RegisterServices.RegisterService.Start(mockEnv.Object, _senparcSetting)
                .UseSenparcGlobal(autoScanExtensionCacheStrategies);

            registerAction?.Invoke(registerService);


            registerService.ChangeDefaultCacheNamespace("Senparc.AI Tests");

            //����ȫ��ʹ��Redis���棨���裬������
            var redisConfigurationStr = _senparcSetting.Cache_Redis_Configuration;
            var useRedis = !string.IsNullOrEmpty(redisConfigurationStr) && redisConfigurationStr != "#{Cache_Redis_Configuration}#"/*Ĭ��ֵ��������*/;
            if (useRedis)//����Ϊ�˷��㲻ͬ�����Ŀ����߽������ã��������жϵķ�ʽ��ʵ�ʿ�������һ����ȷ���ģ������if�������Ժ���
            {
                /* ˵����
                 * 1��Redis �������ַ�����Ϣ��� Config.SenparcSetting.Cache_Redis_Configuration �Զ���ȡ��ע�ᣬ�粻��Ҫ�޸ģ��·��������Ժ���
                /* 2�������ֶ��޸ģ�����ͨ���·� SetConfigurationOption �����ֶ����� Redis ������Ϣ�����޸����ã����������ã�
                 */
                Senparc.CO2NET.Cache.Redis.Register.SetConfigurationOption(redisConfigurationStr);
                Console.WriteLine("��� Redis ����");


                //���»�������ȫ�ֻ�������Ϊ Redis
                Senparc.CO2NET.Cache.Redis.Register.UseKeyValueRedisNow();//��ֵ�Ի�����ԣ��Ƽ���
                Console.WriteLine("���� Redis UseKeyValue ����");

                //Senparc.CO2NET.Cache.Redis.Register.UseHashRedisNow();//HashSet�����ʽ�Ļ������

                //Ҳ����ͨ�����·�ʽ�Զ��嵱ǰ��Ҫ���õĻ������
                //CacheStrategyFactory.RegisterObjectCacheStrategy(() => RedisObjectCacheStrategy.Instance);//��ֵ��
                //CacheStrategyFactory.RegisterObjectCacheStrategy(() => RedisHashSetObjectCacheStrategy.Instance);//HashSet
            }
        }
    }
}
