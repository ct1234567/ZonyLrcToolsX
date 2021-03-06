﻿using Newtonsoft.Json;
using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace Zony.Lib.Net
{
    public class HttpMethodUtils
    {
        private readonly HttpClient m_client;

        public HttpMethodUtils() => m_client = new HttpClient();

        /// <summary>
        /// 对目标URL进行HTTP-GET请求
        /// </summary>
        /// <param name="url">目标URL</param>
        /// <param name="parameters">要提交的参数</param>
        /// <param name="referer">来源地址</param>
        /// <returns>服务器响应结果</returns>
        public string Get(string url, object parameters = null, string referer = null)
        {
            var _req = new HttpRequestMessage()
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri($"{url}{BaseFormBuildParameters(parameters)}")
            };

            if (referer != null) _req.Headers.Referrer = new Uri(referer);

            using (var _msg = m_client.SendAsync(_req).Result)
            {
                if (_msg.StatusCode != HttpStatusCode.OK) return string.Empty;
                return _msg.Content.ReadAsStringAsync().Result;
            };
        }

        /// <summary>
        /// 对目标 URL 进行异步 HTTP-GET 请求
        /// </summary>
        /// <param name="url">目标URL</param>
        /// <param name="parameters">要提交的参数</param>
        /// <param name="referer">来源地址</param>
        /// <returns>服务器响应结果</returns>
        /// <returns></returns>
        public async Task<TJsonModel> GetAsync<TJsonModel>(string url, object parameters = null, string referer = null)
        {
            var _request = new HttpRequestMessage()
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri($"{url}{BaseFormBuildParameters(parameters)}")
            };

            if (referer != null) _request.Headers.Referrer = new Uri(referer);
            using (var _msg = await m_client.SendAsync(_request))
            {
                if (_msg.StatusCode != HttpStatusCode.OK) return default(TJsonModel);
                return await Task.Run(async () =>
                {
                    var _jsonStr = await _msg.Content.ReadAsStringAsync();
                    return JsonConvert.DeserializeObject<TJsonModel>(_jsonStr);
                });

            }
        }

        /// <summary>
        /// 对目标URL进行HTTP-POST请求
        /// </summary>
        /// <param name="url">目标URL</param>
        /// <param name="parameters">待提交的参数</param>
        /// <param name="referer">来源地址</param>
        /// <param name="mediaTypeValue">提交的内容类型</param>
        /// <returns>服务器响应结果</returns>
        public string Post(string url, object parameters = null, string referer = null, string mediaTypeValue = null)
        {
            string _postData = string.Empty;

            var _req = new HttpRequestMessage()
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri(url)
            };

            if (referer != null) _req.Headers.Referrer = new Uri(referer);
            // 请求内容构造
            if (mediaTypeValue == "application/json") _postData = BaseJsonBuildParameters(parameters);
            else _postData = BaseFormBuildParameters(parameters);
            _req.Content = new StringContent(_postData);
            if (mediaTypeValue != null) _req.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(mediaTypeValue);


            using (var _res = m_client.SendAsync(_req).Result)
            {
                if (_res.StatusCode != HttpStatusCode.OK) return string.Empty;
                return _res.Content.ReadAsStringAsync().Result;
            }
        }

        /// <summary>
        /// 对目标URL进行HTTP-GET请求，并对结果进行序列化操作
        /// </summary>
        /// <param name="url">目标URL</param>
        /// <param name="parameters">要提交的参数</param>
        /// <param name="referer">来源地址</param>
        /// <returns>成功序列化的对象</returns>
        public TJsonModel Get<TJsonModel>(string url, object parameters = null, string referer = null)
        {
            return JsonConvert.DeserializeObject<TJsonModel>(Get(url, parameters, referer));
        }

        /// <summary>
        /// 对目标URL进行HTTP-POST请求，并对结果进行序列化操作
        /// </summary>
        /// <param name="url">目标URL</param>
        /// <param name="parameters">待提交的参数</param>
        /// <param name="referer">来源地址</param>
        /// <param name="mediaTypeValue">提交的内容类型</param>
        /// <returns>成功序列化的对象</returns>
        public TJsonModel Post<TJsonModel>(string url, object parameters = null, string referer = null, string mediaTypeValue = null)
        {
            return JsonConvert.DeserializeObject<TJsonModel>(Post(url, parameters, referer, mediaTypeValue));
        }

        /// <summary>
        /// 对目标 URL 进行异步 HTTP-POST 请求，并对结果进行序列化操作
        /// </summary>
        /// <param name="url">目标URL</param>
        /// <param name="parameters">待提交的参数</param>
        /// <param name="referer">来源地址</param>
        /// <param name="mediaTypeValue">提交的内容类型</param>
        /// <returns>成功序列化的对象</returns>
        public async Task<TJsonModel> PostAsync<TJsonModel>(string url, object parameters = null, string referer = null, string mediaTypeValue = null)
        {
            string postData = string.Empty;
            var request = new HttpRequestMessage()
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri(url)
            };

            if (mediaTypeValue == "application/json") postData = BaseFormBuildParameters(parameters);
            else postData = BaseJsonBuildParameters(parameters);

            if (!string.IsNullOrEmpty(postData)) request.Content = new StringContent(postData);
            if (!string.IsNullOrEmpty(referer)) request.Headers.Referrer = new Uri(referer);
            if (!string.IsNullOrEmpty(mediaTypeValue)) request.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(mediaTypeValue);

            using (var response = await m_client.SendAsync(request))
            {
                if (response.StatusCode != HttpStatusCode.OK) return default(TJsonModel);
                return JsonConvert.DeserializeObject<TJsonModel>(await response.Content.ReadAsStringAsync());
            }
        }

        /// <summary>
        /// 构建URL编码字符串
        /// </summary>
        /// <param name="srcText">待编码的字符串</param>
        /// <param name="encoding">编码方式</param>
        [Obsolete("这个方法会导致将英文编码，会产生歌曲无法正常搜索的情况!")]
        public string URL_Encoding_Old(string srcText, Encoding encoding)
        {
            StringBuilder _builder = new StringBuilder();
            byte[] _bytes = encoding.GetBytes(srcText);

            foreach (var _byte in _bytes)
            {
                _builder.Append('%').Append(_byte.ToString("x2"));
            }

            return _builder.ToString().TrimEnd('%');
        }

        /// <summary>
        /// 构建URL编码字符串
        /// </summary>
        /// <param name="srcText">待编码的字符串</param>
        /// <param name="encoding">编码方式</param>
        public string URL_Encoding(string srcText, Encoding encoding)
        {
            return HttpUtility.UrlEncode(srcText, encoding);
        }

        /// <summary>
        /// 基于 QueryString 形式构建查询参数
        /// </summary>
        /// <param name="parameters">参数对象</param>
        private string BaseFormBuildParameters(object parameters)
        {
            if (parameters == null) return string.Empty;
            var _type = parameters.GetType();
            if (_type == typeof(string)) return parameters as string;

            var _properties = _type.GetProperties();
            StringBuilder _paramBuidler = new StringBuilder("?");

            // 反射构建参数
            foreach (var _property in _properties)
            {
                _paramBuidler.Append($"{_property.Name}={_property.GetValue(parameters)}&");
            }
            return _paramBuidler.ToString().Trim('&');
        }

        /// <summary>
        /// 基于 JSON 形式构建查询参数
        /// </summary>
        /// <param name="parameters">参数对象</param>
        private string BaseJsonBuildParameters(object parameters)
        {
            if (parameters == null) return string.Empty;
            if (parameters.GetType() == typeof(string)) return parameters as string;
            return JsonConvert.SerializeObject(parameters);
        }
    }
}