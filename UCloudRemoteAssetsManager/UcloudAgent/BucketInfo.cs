using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UcloudAgent
{
    class BucketInfo
    {
        string _bucketName;
        string _publicKey;
        string _privateKey;
        string _proxyHost;
        string _apiHost;

        public string bucketName { get { return _bucketName; } }
        public string publicKey { get { return _publicKey; } }
        public string privateKey { get { return _privateKey; } }
        public string proxyHost { get { return _proxyHost; } }
        public string apiHost { get { return _apiHost; } }

        public BucketInfo(string p_bucketName,string p_publicKey, string p_privateKey, string p_proxyHost = "www.cn-bj.ufileos.com",string p_apiHost = "api.spark.ucloud.cn")
        {
            this._bucketName = p_bucketName;
            this._publicKey = p_publicKey;
            this._privateKey = p_privateKey;
            this._proxyHost = p_proxyHost;
            this._apiHost = p_apiHost;
        }
        public override string ToString()
        {
            return "bucketName \t= " + bucketName + "\n" +
                   "publicKey \t= " + publicKey + "\n" +
                   "privateKey \t= " + privateKey + "\n" +
                   "proxyHost \t= " + proxyHost + "\n" +
                   "apiHost \t= " + apiHost;
        }
    }
}
