﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.233
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace bycar3.NbrbServiceReference
{
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    [System.ServiceModel.ServiceContractAttribute(Namespace = "http://www.nbrb.by/", ConfigurationName = "NbrbServiceReference.ExRatesSoap")]
    public interface ExRatesSoap
    {
        [System.ServiceModel.OperationContractAttribute(Action = "http://www.nbrb.by/CurrenciesRefDaily", ReplyAction = "*")]
        [System.ServiceModel.XmlSerializerFormatAttribute(SupportFaults = true)]
        System.Data.DataSet CurrenciesRefDaily();

        [System.ServiceModel.OperationContractAttribute(Action = "http://www.nbrb.by/CurrenciesRefMonthly", ReplyAction = "*")]
        [System.ServiceModel.XmlSerializerFormatAttribute(SupportFaults = true)]
        System.Data.DataSet CurrenciesRefMonthly();

        [System.ServiceModel.OperationContractAttribute(Action = "http://www.nbrb.by/StartDate", ReplyAction = "*")]
        [System.ServiceModel.XmlSerializerFormatAttribute(SupportFaults = true)]
        System.DateTime StartDate(int Periodicity);

        [System.ServiceModel.OperationContractAttribute(Action = "http://www.nbrb.by/LastDailyExRatesDate", ReplyAction = "*")]
        [System.ServiceModel.XmlSerializerFormatAttribute(SupportFaults = true)]
        System.DateTime LastDailyExRatesDate();

        [System.ServiceModel.OperationContractAttribute(Action = "http://www.nbrb.by/LastMonthlyExRatesDate", ReplyAction = "*")]
        [System.ServiceModel.XmlSerializerFormatAttribute(SupportFaults = true)]
        System.DateTime LastMonthlyExRatesDate();

        [System.ServiceModel.OperationContractAttribute(Action = "http://www.nbrb.by/ExRatesDaily", ReplyAction = "*")]
        [System.ServiceModel.XmlSerializerFormatAttribute(SupportFaults = true)]
        System.Data.DataSet ExRatesDaily(System.DateTime onDate);

        [System.ServiceModel.OperationContractAttribute(Action = "http://www.nbrb.by/ExRatesMonthly", ReplyAction = "*")]
        [System.ServiceModel.XmlSerializerFormatAttribute(SupportFaults = true)]
        System.Data.DataSet ExRatesMonthly(System.DateTime onDate);

        [System.ServiceModel.OperationContractAttribute(Action = "http://www.nbrb.by/ExRatesDyn", ReplyAction = "*")]
        [System.ServiceModel.XmlSerializerFormatAttribute(SupportFaults = true)]
        System.Data.DataSet ExRatesDyn(int curId, System.DateTime fromDate, System.DateTime toDate);

        [System.ServiceModel.OperationContractAttribute(Action = "http://www.nbrb.by/CurrenciesRef", ReplyAction = "*")]
        [System.ServiceModel.XmlSerializerFormatAttribute(SupportFaults = true)]
        System.Data.DataSet CurrenciesRef(int Periodicity);

        [System.ServiceModel.OperationContractAttribute(Action = "http://www.nbrb.by/MetalsLastDate", ReplyAction = "*")]
        [System.ServiceModel.XmlSerializerFormatAttribute(SupportFaults = true)]
        System.DateTime MetalsLastDate();

        [System.ServiceModel.OperationContractAttribute(Action = "http://www.nbrb.by/MetalsRef", ReplyAction = "*")]
        [System.ServiceModel.XmlSerializerFormatAttribute(SupportFaults = true)]
        System.Data.DataSet MetalsRef();

        [System.ServiceModel.OperationContractAttribute(Action = "http://www.nbrb.by/MetalsPrices", ReplyAction = "*")]
        [System.ServiceModel.XmlSerializerFormatAttribute(SupportFaults = true)]
        System.Data.DataSet MetalsPrices(int MetalId, System.DateTime fromDate, System.DateTime toDate);
    }

    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    public interface ExRatesSoapChannel : bycar3.NbrbServiceReference.ExRatesSoap, System.ServiceModel.IClientChannel
    {
    }

    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    public partial class ExRatesSoapClient : System.ServiceModel.ClientBase<bycar3.NbrbServiceReference.ExRatesSoap>, bycar3.NbrbServiceReference.ExRatesSoap
    {
        public ExRatesSoapClient()
        {
        }

        public ExRatesSoapClient(string endpointConfigurationName) :
            base(endpointConfigurationName)
        {
        }

        public ExRatesSoapClient(string endpointConfigurationName, string remoteAddress) :
            base(endpointConfigurationName, remoteAddress)
        {
        }

        public ExRatesSoapClient(string endpointConfigurationName, System.ServiceModel.EndpointAddress remoteAddress) :
            base(endpointConfigurationName, remoteAddress)
        {
        }

        public ExRatesSoapClient(System.ServiceModel.Channels.Binding binding, System.ServiceModel.EndpointAddress remoteAddress) :
            base(binding, remoteAddress)
        {
        }

        public System.Data.DataSet CurrenciesRefDaily()
        {
            return base.Channel.CurrenciesRefDaily();
        }

        public System.Data.DataSet CurrenciesRefMonthly()
        {
            return base.Channel.CurrenciesRefMonthly();
        }

        public System.DateTime StartDate(int Periodicity)
        {
            return base.Channel.StartDate(Periodicity);
        }

        public System.DateTime LastDailyExRatesDate()
        {
            return base.Channel.LastDailyExRatesDate();
        }

        public System.DateTime LastMonthlyExRatesDate()
        {
            return base.Channel.LastMonthlyExRatesDate();
        }

        public System.Data.DataSet ExRatesDaily(System.DateTime onDate)
        {
            return base.Channel.ExRatesDaily(onDate);
        }

        public System.Data.DataSet ExRatesMonthly(System.DateTime onDate)
        {
            return base.Channel.ExRatesMonthly(onDate);
        }

        public System.Data.DataSet ExRatesDyn(int curId, System.DateTime fromDate, System.DateTime toDate)
        {
            return base.Channel.ExRatesDyn(curId, fromDate, toDate);
        }

        public System.Data.DataSet CurrenciesRef(int Periodicity)
        {
            return base.Channel.CurrenciesRef(Periodicity);
        }

        public System.DateTime MetalsLastDate()
        {
            return base.Channel.MetalsLastDate();
        }

        public System.Data.DataSet MetalsRef()
        {
            return base.Channel.MetalsRef();
        }

        public System.Data.DataSet MetalsPrices(int MetalId, System.DateTime fromDate, System.DateTime toDate)
        {
            return base.Channel.MetalsPrices(MetalId, fromDate, toDate);
        }
    }
}