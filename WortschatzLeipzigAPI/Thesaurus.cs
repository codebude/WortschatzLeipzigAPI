using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using WortschatzLeipzigAPI.WortschatzWebAPI;

namespace WortschatzLeipzigAPI
{
    public class Thesaurus
    {
       
        public static List<string> GetSynonyms(string inputWord, int maxSynonyms = 500, string corpus = "de")
        {
            try
            {
                BasicHttpBinding basicAuthBinding = new BasicHttpBinding(BasicHttpSecurityMode.TransportCredentialOnly);
                basicAuthBinding.Security.Transport.ClientCredentialType = HttpClientCredentialType.Basic;
                EndpointAddress basicAuthEndpoint = new EndpointAddress("http://wortschatz.uni-leipzig.de:8100/axis/services/Thesaurus");

                ThesaurusClient service = new ThesaurusClient(basicAuthBinding, basicAuthEndpoint);
                var requestInterceptor = new InspectorBehavior();
                service.Endpoint.Behaviors.Add(requestInterceptor);

                service.ClientCredentials.UserName.UserName = "anonymous";
                service.ClientCredentials.UserName.Password = "anonymous";

                DataMatrix matrix = new DataMatrix();
                List<DataVector> vector = new List<DataVector>();
                DataVector dv = new DataVector();
                dv.Add("Wort");
                dv.Add(inputWord);
                vector.Add(dv);
                dv = new DataVector();
                dv.Add("Limit");
                dv.Add(maxSynonyms.ToString());
                vector.Add(dv);
                matrix.AddRange(vector);

                RequestParameter request = new RequestParameter();
                request.corpus = corpus;
                request.parameters = matrix;


                using (OperationContextScope scope = new OperationContextScope(service.InnerChannel))
                {
                    var httpRequestProperty = new HttpRequestMessageProperty();
                    httpRequestProperty.Headers[System.Net.HttpRequestHeader.Authorization] = "Basic " + Convert.ToBase64String(Encoding.ASCII.GetBytes(service.ClientCredentials.UserName.UserName + ":" + service.ClientCredentials.UserName.Password));
                    OperationContext.Current.OutgoingMessageProperties[HttpRequestMessageProperty.Name] = httpRequestProperty;

                    ResponseParameter resp = service.execute(request);
                    string requestXML = requestInterceptor.LastRequestXML;
                    string responseXML = requestInterceptor.LastResponseXML;

                    Regex reg = new Regex("<ns\\d+:dataRow>(.*?)</ns\\d+:dataRow>");
                    MatchCollection mc = reg.Matches(responseXML);

                    List<string> synonymList = new List<string>();
                    
                    if (mc.Count > 0)
                        synonymList.AddRange((from Match m in mc select m.Groups[1].Value));

                    return synonymList;
                }


            }
            catch (Exception e)
            {
                throw e;
            }
        }

        private class InspectorBehavior : IEndpointBehavior
        {
            public string LastRequestXML
            {
                get
                {
                    return myMessageInspector.LastRequestXML;
                }
            }

            public string LastResponseXML
            {
                get
                {
                    return myMessageInspector.LastResponseXML;
                }
            }


            private MyMessageInspector myMessageInspector = new MyMessageInspector();
            public void AddBindingParameters(ServiceEndpoint endpoint, System.ServiceModel.Channels.BindingParameterCollection bindingParameters)
            {

            }

            public void ApplyDispatchBehavior(ServiceEndpoint endpoint, EndpointDispatcher endpointDispatcher)
            {

            }

            public void Validate(ServiceEndpoint endpoint)
            {

            }


            public void ApplyClientBehavior(ServiceEndpoint endpoint, ClientRuntime clientRuntime)
            {
                clientRuntime.MessageInspectors.Add(myMessageInspector);
            }
        }

        private class MyMessageInspector : IClientMessageInspector
        {
            public string LastRequestXML { get; private set; }
            public string LastResponseXML { get; private set; }
            public void AfterReceiveReply(ref System.ServiceModel.Channels.Message reply, object correlationState)
            {
                LastResponseXML = reply.ToString();
            }

            public object BeforeSendRequest(ref System.ServiceModel.Channels.Message request, System.ServiceModel.IClientChannel channel)
            {
                LastRequestXML = request.ToString();
                return request;
            }
        }
    }
}
