using System;
using System.Runtime.Caching;
using Lip2p.Common;

namespace Lip2p.Core
{
    /// <summary>
    /// 数据访问类:站点配置
    /// </summary>
    public static class ConfigLoader 
    {
        //private static object lockHelper = new object();

        public static void CleanCache()
        {
            MemoryCache.Default.Remove("site_config");
            MemoryCache.Default.Remove("user_config");
        }

        /// <summary>
        ///  读取站点配置文件，不要将 Config 存到静态变量，因为这个方法已经做了缓存控制
        /// </summary>
        /// <param name="loadOutsideProject"></param>
        public static siteconfig loadSiteConfig(bool loadOutsideProject = true)
        {
            try
            {
                var configCache = (siteconfig)MemoryCache.Default.Get("site_config");
                if (configCache != null) return configCache;

                configCache = SerializationHelper.Load<siteconfig>(Utils.GetXmlMapPath(DTKeys.FILE_SITE_XML_CONFING, loadOutsideProject));
                MemoryCache.Default.Set("site_config", configCache, DateTime.Now.AddMinutes(20)); // 20 分钟超时
                return configCache;
            }
            catch
            {
                return null;
            }
        }

        public static userconfig loadUserConfig(bool loadOutsideProject = true)
        {
            try
            {
                var configCache = (userconfig)MemoryCache.Default.Get("user_config");
                if (configCache != null) return configCache;

                configCache = SerializationHelper.Load<userconfig>(Utils.GetXmlMapPath(DTKeys.FILE_USER_XML_CONFING, loadOutsideProject));
                MemoryCache.Default.Set("user_config", configCache, DateTime.Now.AddMinutes(20)); // 20 分钟超时
                return configCache;
            }
            catch
            {
                return null;
            }
        }

        /*/// <summary>
        /// 写入站点配置文件
        /// </summary>
        public static Config save(Config model)
        {
            lock (lockHelper)
            {
                SerializationHelper.Save(model, Utils.GetXmlMapPath(DTKeys.FILE_SITE_XML_CONFING));
            }
            return model;
        }*/

    }

    /// <summary>
    /// 站点配置实体类
    /// </summary>
    [Serializable]
    public class siteconfig
    {
        private string _webname = "";
        private string _weburl = "";
        private string _weblogo = "";
        private string _webcompany = "";
        private string _webaddress = "";
        private string _webtel = "";
        private string _webfax = "";
        private string _webmail = "";
        private string _webcrod = "";
        private string _webtitle = "";
        private string _webkeyword = "";
        private string _webdescription = "";
        private string _webcopyright = "";

        private string _webpath = "";
        private string _webmanagepath = "";
        private int _staticstatus = 0;
        private string _staticextension = "";
        private int _mobilestatus = 1;
        private string _mobiledomain = "";
        private int _memberstatus = 1;
        private int _commentstatus = 0;
        private int _logstatus = 0;
        private int _webstatus = 1;
        private string _webclosereason = "";
        private string _webcountcode = "";

        private string _smsapiurl = "";
        private string _smsusername = "";
        private string _smspassword = "";

        private string _emailsmtp = "";
        private int _emailport = 25;
        private string _emailfrom = "";
        private string _emailusername = "";
        private string _emailpassword = "";
        private string _emailnickname = "";

        private string _filepath = "";
        private int _filesave = 1;
        private int _fileremote = 0;
        private string _fileextension = "";
        private int _attachsize = 0;
        private int _imgsize = 0;
        private int _imgmaxheight = 0;
        private int _imgmaxwidth = 0;
        private int _thumbnailheight = 0;
        private int _thumbnailwidth = 0;
        private int _watermarktype = 0;
        private int _watermarkposition = 9;
        private int _watermarkimgquality = 80;
        private string _watermarkpic = "";
        private int _watermarktransparency = 10;
        private string _watermarktext = "";
        private string _watermarkfont = "";
        private int _watermarkfontsize = 12;

        private string _sysdatabaseprefix = "dt_";
        private string _sysencryptstring = "Lip2p";

        private int _enableAutoRepay = 0;
        private string _autoRepayTime = "9:00:00";
        private int _sendShortMsgAfterRepay = 1;
        private int _sendRepayAnnounceAfterRepay = 1;

        #region 网站基本信息==================================
        /// <summary>
        /// 网站名称
        /// </summary>
        public string webname
        {
            get { return _webname; }
            set { _webname = value; }
        }
        /// <summary>
        /// 网站域名
        /// </summary>
        public string weburl
        {
            get { return _weburl; }
            set { _weburl = value; }
        }
        /// <summary>
        /// 网站LOGO
        /// </summary>
        public string weblogo
        {
            get { return _weblogo; }
            set { _weblogo = value; }
        }
        /// <summary>
        /// 公司名称
        /// </summary>
        public string webcompany
        {
            get { return _webcompany; }
            set { _webcompany = value; }
        }
        /// <summary>
        /// 通讯地址
        /// </summary>
        public string webaddress
        {
            get { return _webaddress; }
            set { _webaddress = value; }
        }
        /// <summary>
        /// 联系电话
        /// </summary>
        public string webtel
        {
            get { return _webtel; }
            set { _webtel = value; }
        }
        /// <summary>
        /// 传真号码
        /// </summary>
        public string webfax
        {
            get { return _webfax; }
            set { _webfax = value; }
        }
        /// <summary>
        /// 管理员邮箱
        /// </summary>
        public string webmail
        {
            get { return _webmail; }
            set { _webmail = value; }
        }
        /// <summary>
        /// 网站备案号
        /// </summary>
        public string webcrod
        {
            get { return _webcrod; }
            set { _webcrod = value; }
        }
        /// <summary>
        /// 网站首页标题
        /// </summary>
        public string webtitle
        {
            get { return _webtitle; }
            set { _webtitle = value; }
        }
        /// <summary>
        /// 页面关健词
        /// </summary>
        public string webkeyword
        {
            get { return _webkeyword; }
            set { _webkeyword = value; }
        }
        /// <summary>
        /// 页面描述
        /// </summary>
        public string webdescription
        {
            get { return _webdescription; }
            set { _webdescription = value; }
        }
        /// <summary>
        /// 网站版权信息
        /// </summary>
        public string webcopyright
        {
            get { return _webcopyright; }
            set { _webcopyright = value; }
        }
        #endregion

        #region 功能权限设置==================================
        /// <summary>
        /// 网站安装目录
        /// </summary>
        public string webpath
        {
            get { return _webpath; }
            set { _webpath = value; }
        }
        /// <summary>
        /// 网站管理目录
        /// </summary>
        public string webmanagepath
        {
            get { return _webmanagepath; }
            set { _webmanagepath = value; }
        }
        /// <summary>
        /// 是否开启生成静态
        /// </summary>
        public int staticstatus
        {
            get { return _staticstatus; }
            set { _staticstatus = value; }
        }
        /// <summary>
        /// 生成静态扩展名
        /// </summary>
        public string staticextension
        {
            get { return _staticextension; }
            set { _staticextension = value; }
        }
        /// <summary>
        /// 手机网站状态0关闭1开启
        /// </summary>
        public int mobilestatus
        {
            get { return _mobilestatus; }
            set { _mobilestatus = value; }
        }
        /// <summary>
        /// 手机网站绑定域名
        /// </summary>
        public string mobiledomain
        {
            get { return _mobiledomain; }
            set { _mobiledomain = value; }
        }
        /// <summary>
        /// 开启会员功能
        /// </summary>
        public int memberstatus
        {
            get { return _memberstatus; }
            set { _memberstatus = value; }
        }
        /// <summary>
        /// 开启评论审核
        /// </summary>
        public int commentstatus
        {
            get { return _commentstatus; }
            set { _commentstatus = value; }
        }
        /// <summary>
        /// 后台管理日志
        /// </summary>
        public int logstatus
        {
            get { return _logstatus; }
            set { _logstatus = value; }
        }
        /// <summary>
        /// 是否关闭网站
        /// </summary>
        public int webstatus
        {
            get { return _webstatus; }
            set { _webstatus = value; }
        }
        /// <summary>
        /// 关闭原因描述
        /// </summary>
        public string webclosereason
        {
            get { return _webclosereason; }
            set { _webclosereason = value; }
        }
        /// <summary>
        /// 网站统计代码
        /// </summary>
        public string webcountcode
        {
            get { return _webcountcode; }
            set { _webcountcode = value; }
        }
        #endregion

        #region 短信平台设置==================================
        /// <summary>
        /// 短信API地址
        /// </summary>
        public string smsapiurl
        {
            get { return _smsapiurl; }
            set { _smsapiurl = value; }
        }
        /// <summary>
        /// 短信平台登录账户名
        /// </summary>
        public string smsusername
        {
            get { return _smsusername; }
            set { _smsusername = value; }
        }
        /// <summary>
        /// 短信平台登录密码
        /// </summary>
        public string smspassword
        {
            get { return _smspassword; }
            set { _smspassword = value; }
        }
        #endregion

        #region 邮件发送设置==================================
        /// <summary>
        /// STMP服务器
        /// </summary>
        public string emailsmtp
        {
            get { return _emailsmtp; }
            set { _emailsmtp = value; }
        }
        /// <summary>
        /// SMTP端口
        /// </summary>
        public int emailport
        {
            get { return _emailport; }
            set { _emailport = value; }
        }
        /// <summary>
        /// 发件人地址
        /// </summary>
        public string emailfrom
        {
            get { return _emailfrom; }
            set { _emailfrom = value; }
        }
        /// <summary>
        /// 邮箱账号
        /// </summary>
        public string emailusername
        {
            get { return _emailusername; }
            set { _emailusername = value; }
        }
        /// <summary>
        /// 邮箱密码
        /// </summary>
        public string emailpassword
        {
            get { return _emailpassword; }
            set { _emailpassword = value; }
        }
        /// <summary>
        /// 发件人昵称
        /// </summary>
        public string emailnickname
        {
            get { return _emailnickname; }
            set { _emailnickname = value; }
        }
        #endregion

        #region 文件上传设置==================================
        /// <summary>
        /// 附件上传目录
        /// </summary>
        public string filepath
        {
            get { return _filepath; }
            set { _filepath = value; }
        }
        /// <summary>
        /// 附件保存方式
        /// </summary>
        public int filesave
        {
            get { return _filesave; }
            set { _filesave = value; }
        }
        /// <summary>
        /// 编辑器远程图片上传
        /// </summary>
        public int fileremote
        {
            get { return _fileremote; }
            set { _fileremote = value; }
        }
        /// <summary>
        /// 附件上传类型
        /// </summary>
        public string fileextension
        {
            get { return _fileextension; }
            set { _fileextension = value; }
        }
        /// <summary>
        /// 文件上传大小
        /// </summary>
        public int attachsize
        {
            get { return _attachsize; }
            set { _attachsize = value; }
        }
        /// <summary>
        /// 图片上传大小
        /// </summary>
        public int imgsize
        {
            get { return _imgsize; }
            set { _imgsize = value; }
        }
        /// <summary>
        /// 图片最大高度(像素)
        /// </summary>
        public int imgmaxheight
        {
            get { return _imgmaxheight; }
            set { _imgmaxheight = value; }
        }
        /// <summary>
        /// 图片最大宽度(像素)
        /// </summary>
        public int imgmaxwidth
        {
            get { return _imgmaxwidth; }
            set { _imgmaxwidth = value; }
        }
        /// <summary>
        /// 生成缩略图高度(像素)
        /// </summary>
        public int thumbnailheight
        {
            get { return _thumbnailheight; }
            set { _thumbnailheight = value; }
        }
        /// <summary>
        /// 生成缩略图宽度(像素)
        /// </summary>
        public int thumbnailwidth
        {
            get { return _thumbnailwidth; }
            set { _thumbnailwidth = value; }
        }
        /// <summary>
        /// 图片水印类型
        /// </summary>
        public int watermarktype
        {
            get { return _watermarktype; }
            set { _watermarktype = value; }
        }
        /// <summary>
        /// 图片水印位置
        /// </summary>
        public int watermarkposition
        {
            get { return _watermarkposition; }
            set { _watermarkposition = value; }
        }
        /// <summary>
        /// 图片生成质量
        /// </summary>
        public int watermarkimgquality
        {
            get { return _watermarkimgquality; }
            set { _watermarkimgquality = value; }
        }
        /// <summary>
        /// 图片水印文件
        /// </summary>
        public string watermarkpic
        {
            get { return _watermarkpic; }
            set { _watermarkpic = value; }
        }
        /// <summary>
        /// 水印透明度
        /// </summary>
        public int watermarktransparency
        {
            get { return _watermarktransparency; }
            set { _watermarktransparency = value; }
        }
        /// <summary>
        /// 水印文字
        /// </summary>
        public string watermarktext
        {
            get { return _watermarktext; }
            set { _watermarktext = value; }
        }
        /// <summary>
        /// 文字字体
        /// </summary>
        public string watermarkfont
        {
            get { return _watermarkfont; }
            set { _watermarkfont = value; }
        }
        /// <summary>
        /// 文字大小(像素)
        /// </summary>
        public int watermarkfontsize
        {
            get { return _watermarkfontsize; }
            set { _watermarkfontsize = value; }
        }
        #endregion

        #region 安装初始化设置================================
        /// <summary>
        /// 数据库表前缀
        /// </summary>
        public string sysdatabaseprefix
        {
            get { return _sysdatabaseprefix; }
            set { _sysdatabaseprefix = value; }
        }
        /// <summary>
        /// 加密字符串
        /// </summary>
        public string sysencryptstring
        {
            get { return _sysencryptstring; }
            set { _sysencryptstring = value; }
        }
        #endregion

        /// <summary>
        /// 是否自动发送放款通知短信
        /// </summary>
        public int enableAutoRepay
        {
            get { return _enableAutoRepay; }
            set { _enableAutoRepay = value; }
        }
        /// <summary>
        /// 每天自动放款时间
        /// </summary>
        public string autoRepayTime
        {
            get { return _autoRepayTime; }
            set { _autoRepayTime = value; }
        }
        /// <summary>
        /// 是否自动发送放款通知短信
        /// </summary>
        public int sendShortMsgAfterRepay
        {
            get { return _sendShortMsgAfterRepay; }
            set { _sendShortMsgAfterRepay = value; }
        }
        /// <summary>
        /// 是否自动发送兑付公告
        /// </summary>
        public int sendRepayAnnounceAfterRepay
        {
            get { return _sendRepayAnnounceAfterRepay; }
            set { _sendRepayAnnounceAfterRepay = value; }
        }
    }

    /// <summary>
    /// 会员配置信息
    /// </summary>
    [Serializable]
    public class userconfig
    {
        private int _regstatus = 0;
        private int _regverify = 0;
        private int _regmsgstatus = 0;
        private string _regmsgtxt = "";
        private string _regkeywords = "";
        private int _regctrl = 0;
        private int _regsmsexpired = 0;
        private int _regemailexpired = 0;
        private int _regemailditto = 0;
        private int _mobilelogin = 0;
        private int _emaillogin = 0;
        private int _regrules = 0;
        private string _regrulestxt = "";

        private int _invitecodeexpired = 0;
        private int _invitecodecount = 0;
        private int _invitecodenum = 10;
        private decimal _pointcashrate = 0;
        private int _pointinvitenum = 0;
        private int _pointloginnum = 0;

        /// <summary>
        /// 新用户注册设置0关闭注册,1开放注册,2手机短信,3邮件链接,4邀请注册
        /// </summary>
        public int regstatus
        {
            get { return _regstatus; }
            set { _regstatus = value; }
        }
        /// <summary>
        /// 新用户注册验证0无验证,1邮箱验证,2手机验证,3人工审核
        /// </summary>
        public int regverify
        {
            get { return _regverify; }
            set { _regverify = value; }
        }
        /// <summary>
        /// 注册欢迎短信息0不发送1站内短消息2发送邮件3手机短信
        /// </summary>
        public int regmsgstatus
        {
            get { return _regmsgstatus; }
            set { _regmsgstatus = value; }
        }
        /// <summary>
        /// 欢迎短信息内容
        /// </summary>
        public string regmsgtxt
        {
            get { return _regmsgtxt; }
            set { _regmsgtxt = value; }
        }
        /// <summary>
        /// 用户名保留关健字
        /// </summary>
        public string regkeywords
        {
            get { return _regkeywords; }
            set { _regkeywords = value; }
        }
        /// <summary>
        /// IP注册间隔限制0不限制(小时)
        /// </summary>
        public int regctrl
        {
            get { return _regctrl; }
            set { _regctrl = value; }
        }
        /// <summary>
        /// 手机验证码有效期0不限制(分钟)
        /// </summary>
        public int regsmsexpired
        {
            get { return _regsmsexpired; }
            set { _regsmsexpired = value; }
        }
        /// <summary>
        /// 邮件链接有效期0不限制(天)
        /// </summary>
        public int regemailexpired
        {
            get { return _regemailexpired; }
            set { _regemailexpired = value; }
        }
        /// <summary>
        /// 允许同一Email注册不同用户0不允许1允许
        /// </summary>
        public int regemailditto
        {
            get { return _regemailditto; }
            set { _regemailditto = value; }
        }
        /// <summary>
        /// 允许手机号登录0不允许1允许
        /// </summary>
        public int mobilelogin
        {
            get { return _mobilelogin; }
            set { _mobilelogin = value; }
        }
        /// <summary>
        /// 允许邮箱登录0不允许1允许
        /// </summary>
        public int emaillogin
        {
            get { return _emaillogin; }
            set { _emaillogin = value; }
        }
        /// <summary>
        /// 注册许可协议0否1是
        /// </summary>
        public int regrules
        {
            get { return _regrules; }
            set { _regrules = value; }
        }
        /// <summary>
        /// 许可协议内容
        /// </summary>
        public string regrulestxt
        {
            get { return _regrulestxt; }
            set { _regrulestxt = value; }
        }

        /// <summary>
        /// 邀请码使用期限(天)0不限制
        /// </summary>
        public int invitecodeexpired
        {
            get { return _invitecodeexpired; }
            set { _invitecodeexpired = value; }
        }
        /// <summary>
        /// 邀请码可使用次数0无限制
        /// </summary>
        public int invitecodecount
        {
            get { return _invitecodecount; }
            set { _invitecodecount = value; }
        }
        /// <summary>
        /// 每天可申请的邀请码数量0不限制
        /// </summary>
        public int invitecodenum
        {
            get { return _invitecodenum; }
            set { _invitecodenum = value; }
        }
        /// <summary>
        /// 现金/积分兑换比例0禁用
        /// </summary>
        public decimal pointcashrate
        {
            get { return _pointcashrate; }
            set { _pointcashrate = value; }
        }
        /// <summary>
        /// 邀请注册获得积分
        /// </summary>
        public int pointinvitenum
        {
            get { return _pointinvitenum; }
            set { _pointinvitenum = value; }
        }
        /// <summary>
        /// 每天登录获得积分
        /// </summary>
        public int pointloginnum
        {
            get { return _pointloginnum; }
            set { _pointloginnum = value; }
        }

    }
}
