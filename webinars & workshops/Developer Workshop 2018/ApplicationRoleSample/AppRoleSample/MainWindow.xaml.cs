using System;
using System.Configuration;
using System.Windows;
using System.Windows.Media;
using Helpers;
using IdentityModel.Client;

namespace AppRoleSample
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly AppRoleSampleManager _appRoleManager;

        private static string _accessToken;
        private static string _refreshToken;

        public MainWindow()
        {
            InitializeComponent();

            ConnectivitySettings.Server = ConfigurationManager.AppSettings.Get("server");
            ConnectivitySettings.Port = ConfigurationManager.AppSettings.Get("port");
            ConnectivitySettings.Database = ConfigurationManager.AppSettings.Get("database");
            ConnectivitySettings.AppRole = ConfigurationManager.AppSettings.Get("approle");
            ConnectivitySettings.GatewayRestApi = ConfigurationManager.AppSettings.Get("gatewayrestapi");
            ConnectivitySettings.GatewaySoapApi = ConfigurationManager.AppSettings.Get("gatewayrestapi");
            ConnectivitySettings.DSNName = ConfigurationManager.AppSettings.Get("dsnname");

            ConnectivitySettings.Authority = ConfigurationManager.AppSettings.Get("authority");
            ConnectivitySettings.ClientIdentifier = ConfigurationManager.AppSettings.Get("clientIdentifier");
            ConnectivitySettings.ClientSecret = ConfigurationManager.AppSettings.Get("clientSecret");
            ConnectivitySettings.GatewayTokenUri = ConfigurationManager.AppSettings.Get("gatewayTokenUri");
            ConnectivitySettings.Scope = ConfigurationManager.AppSettings.Get("scope");

            AppRoleHelper appRoleHelper = new AppRoleHelper(ConnectivitySettings.GatewayTokenUri);
            _appRoleManager = new AppRoleSampleManager(appRoleHelper);

            SetInitialControlState();
            Loaded += RequestTokenAsync;
        }

        private void RequestTokenAsync(object sender, RoutedEventArgs e)
        {
            var disco = DiscoveryClient.GetAsync(ConnectivitySettings.Authority).Result;
            if (disco.IsError)
            {
                throw new Exception(disco.Error);
            }

            var client = new TokenClient(disco.TokenEndpoint, ConnectivitySettings.ClientIdentifier, ConnectivitySettings.ClientSecret);
            var tokens = client.RequestClientCredentialsAsync(ConnectivitySettings.Scope).Result;
            _accessToken = tokens.AccessToken;
            _refreshToken = tokens.RefreshToken;
        }

        private void GetAppRolePassword_Clicked(object sender, RoutedEventArgs e)
        {
            string details;

            bool appRoleRetreived = _appRoleManager.GetAppRolePassword(_accessToken, out details);
            Status.Text = details;

            if (appRoleRetreived)
            {
                EnableDALControls();
                SetWorkflowIndicator(Workflow.WorkflowState.GetApplicationRole, Visibility.Visible, true); 
            }
            else
                SetWorkflowIndicator(Workflow.WorkflowState.GetApplicationRole, Visibility.Visible, false);
        }

        private void Closed_Clicked(object sender, RoutedEventArgs e)
        {
            string status = "";

            if (_appRoleManager != null)
                _appRoleManager.CloseDBConnection(out status);
    
            if (Status != null)
                Status.Text = status;;

            if (SetAppRolePW != null && Query != null)
            {
                SetWorkflowIndicator(Workflow.WorkflowState.CreateDatabaseConnection, Visibility.Hidden, false);
                SetWorkflowIndicator(Workflow.WorkflowState.SetApplicationRole, Visibility.Hidden, false); 
                DisableSetAppRolePasswordControls();
                DisableQueryControls();
            }
        }

        private void ODBC_Clicked(object sender, RoutedEventArgs e)
        {
            string status;
            bool isODBCConnectionOpen = _appRoleManager.ConnectODBC(out status);

            Status.Text = status;

            if (isODBCConnectionOpen)
            {
                EnableSetAppRolePasswordControls();
                SetWorkflowIndicator(Workflow.WorkflowState.CreateDatabaseConnection, Visibility.Visible, true);
            }
            else
                SetWorkflowIndicator(Workflow.WorkflowState.CreateDatabaseConnection, Visibility.Visible, false);
        }

        private void ADO_Clicked(object sender, RoutedEventArgs e)
        {
            string status;
            bool isADOConnectionOpen = _appRoleManager.ConnectADONet(out status);

            Status.Text = status;

            if (isADOConnectionOpen)
            {
                EnableSetAppRolePasswordControls();
                SetWorkflowIndicator(Workflow.WorkflowState.CreateDatabaseConnection, Visibility.Visible, true);
            }
            else
                SetWorkflowIndicator(Workflow.WorkflowState.CreateDatabaseConnection, Visibility.Visible, false);
        }

        private void SetAppRolePW_Clicked(object sender, RoutedEventArgs e)
        {
            string status;
            bool isAppRoleSet = _appRoleManager.SetAppRolePassword(out status);

            Status.Text = status;

            if (isAppRoleSet)
            {
                EnableQueryControls();
                SetWorkflowIndicator(Workflow.WorkflowState.SetApplicationRole, Visibility.Visible, true);
            }
            else
                SetWorkflowIndicator(Workflow.WorkflowState.SetApplicationRole, Visibility.Visible, false);
        }

        private void Query_Clicked(object sender, RoutedEventArgs e)
        {
            Status.Text = _appRoleManager.Query(QueryTxt.Text);
        }

        //TODO: MVVM
        #region ViewModel

        private void SetInitialControlState()
        {
            DisableDALControls();
            DisableSetAppRolePasswordControls();
            DisableQueryControls();
        }

        private void DisableQueryControls()
        {
            QuerySummaryDescription.IsEnabled = false;
            QueryDetailedDescription.IsEnabled = false;
            QueryTxt.IsEnabled = false;
            Query.IsEnabled = false;
        }

        private void EnableQueryControls()
        {
            QuerySummaryDescription.IsEnabled = true;
            QueryDetailedDescription.IsEnabled = true;
            QueryTxt.IsEnabled = true; 
            Query.IsEnabled = true;
        }

        private void DisableSetAppRolePasswordControls()
        {
            SetAppRoleSummaryDescription.IsEnabled = false;
            SetAppRoleDetailedDescription.IsEnabled = false;
            SetAppRolePW.IsEnabled = false;
        }

        private void EnableSetAppRolePasswordControls()
        {
            SetAppRoleSummaryDescription.IsEnabled = true;
            SetAppRoleDetailedDescription.IsEnabled = true;
            SetAppRolePW.IsEnabled = true;
        }

        private void DisableDALControls()
        {
            DBSummaryDescription.IsEnabled = false;
            DBDetailedDescription.IsEnabled = false;
            Closed.IsEnabled = false;
            ODBC.IsEnabled = false;
            ADO.IsEnabled = false;
        }

        private void EnableDALControls()
        {
            DBSummaryDescription.IsEnabled = true;
            DBDetailedDescription.IsEnabled = true; 
            Closed.IsEnabled = true;
            ODBC.IsEnabled = true;
            ADO.IsEnabled = true;
        }

        private void SetWorkflowIndicator(Workflow.WorkflowState state, Visibility visibility, bool success=true)
        {
            ImageSource source;

            //string strUri = String.Format(@"pack://application:,,,/MyAseemby;component/resources/main titles/{0}", CurrenSelection.TitleImage);
            //imgTitle.Source = new BitmapImage(new Uri(strUri));

            if (success)
                source = (ImageSource)FindResource("ImageSuccess");
            else
                source = (ImageSource)FindResource("ImageError");

            switch (state)
            {
                case Workflow.WorkflowState.GetApplicationRole:
                    GetAppRolePasswordIndicator.Visibility = visibility;
                    GetAppRolePasswordIndicator.Source = source;
                    break;

                case Workflow.WorkflowState.CreateDatabaseConnection:
                    DBConnectionIndicator.Visibility = visibility;
                    DBConnectionIndicator.Source = source;
                    break;

                case Workflow.WorkflowState.SetApplicationRole:
                    SetAppRolePasswordIndicator.Visibility = visibility;
                    SetAppRolePasswordIndicator.Source = source;
                    break;

                case Workflow.WorkflowState.Query:
                    QueryDBIndicator.Visibility = visibility;
                    QueryDBIndicator.Source = source;
                    break;
            }
        }

        #endregion ViewModel

    }
}
