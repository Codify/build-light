using Codify.Vsts.BuildLight.Services;
using Microsoft.ApplicationInsights;

namespace Codify.Vsts.BuildLight.UI
{
    public class BaseViewModel : NotifyPropertyChanged
    {
        public BaseViewModel()
        {
            Telemetry = new TelemetryClient();
        }
 
        public virtual TelemetryClient Telemetry {  get { return GetValue<TelemetryClient>(); } set { SetValue(value); } }

        public virtual BuildService BuildService
        {
            get { return GetValue<BuildService>(); }
            set
            {
                var currentValue = GetValue<BuildService>();
                if (currentValue != null)
                {
                    currentValue.BuildEvent -= OnBuildEvent;
                    currentValue.ServiceEvent -= OnServiceEvent;
                }
                SetValue(value);
                if (value != null)
                {
                    value.BuildEvent += OnBuildEvent;
                    value.ServiceEvent += OnServiceEvent;
                }
            }
        }

        protected async virtual void OnServiceEvent(object sender, BuildEventArgs e)
        {
            if (e.Code == BuildEventCode.BuildInformationRetrievalStart)
            {
                IsBusy = true;
            }
            else if (e.Code == BuildEventCode.BuildInformationRetrievalEnd)
            {
                IsBusy = false;
            }
        }

        protected async virtual void OnBuildEvent(object sender, BuildEventArgs e)
        {
        }
    }
}
