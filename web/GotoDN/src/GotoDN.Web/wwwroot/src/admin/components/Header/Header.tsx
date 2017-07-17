import * as React from 'react';
import {Dropdown, DropdownMenu, DropdownItem} from 'reactstrap';
import {SweetAlertResultEnums, SweetAlerts, SweetAlertTypeEnums} from "../../../commons/sweet-alerts";
import {UserServiceInstance} from "../../services/UserService";
import store from "../../modules/CurrentUser/_store";
import {action_SetCurrentUser} from "../../modules/CurrentUser/_actions";
interface thisState {
  dropdownOpen?: boolean
}
class Header extends React.Component<any, thisState> {

  componentWillMount() {
    this.setState({})
  }

  render() {
    return (<div id="header" className="page-navbar">
        <a href="/" className="navbar-brand hidden-xs hidden-sm logo logo-title">
          <img src="/images/admin_logo.png" className="logo hidden-xs" alt="New Homes Admin"/>
          <img src="/images/admin_logosm.png" className="logo-sm hidden-lg hidden-md" alt="New Homes Admin"/>
        </a>
        <div id="navbar-no-collapse" className="navbar-no-collapse">
          <ul className="nav navbar-nav">
            <li className="toggle-sidebar">
              <a href="#">
                <i className="fa fa-reorder"></i>
                <span className="sr-only">Collapse sidebar</span>
              </a>
            </li>
          </ul>
          <ul className="nav navbar-nav navbar-right">
            <li className="dropdown">
              <a href="#" data-toggle="dropdown">
                <i className="fa fa-cog"></i>
                <span className="sr-only">Settings</span>
              </a>
              <ul className="dropdown-menu dropdown-form dynamic-settings right" role="menu">
                <li><a href="#" className="dropdown-menu-header">Layout settings</a>
                </li>
                <li>
                  <div className="toggle-custom">
                    <label className="toggle" data-on="ON" data-off="OFF">
                      <input type="checkbox" id="fixed-header-toggle" name="fixed-header-toggle" defaultChecked={true}/>
                      <span className="button-checkbox"></span>
                    </label>
                    <label htmlFor="fixed-header-toggle">Fixed header</label>
                  </div>
                </li>
                <li>
                  <div className="toggle-custom">
                    <label className="toggle" data-on="ON" data-off="OFF">
                      <input type="checkbox" id="fixed-left-sidebar" name="fixed-left-sidebar" defaultChecked={true}/>
                      <span className="button-checkbox"></span>
                    </label>
                    <label htmlFor="fixed-left-sidebar">Fixed Left Sidebar</label>
                  </div>
                </li>
              </ul>
            </li>
            <li>
              <a href="/" title="Visit page">
                <i className="fa fa-home"></i>
                <span className="sr-only">Go to HomePage</span>
              </a>
            </li>
            <li>
              <a onClick={() => this.logOut()}>
                <i className="fa fa-power-off" title="Sign out"></i>
                <span className="sr-only">Logout</span>
              </a>
            </li>
          </ul>
        </div>
      </div>
    )
  }

  private async logOut() {
    let result = await SweetAlerts.show({
      title: 'Sure ?',
      text: 'Wanna log out?',
      type: SweetAlertTypeEnums.Warning,
      showCancelButton: true,
      confirmButtonText: 'Yes, sign me out!',

    });
    if (result == SweetAlertResultEnums.Confirm) {

      let logOutResult = await UserServiceInstance.LogOut();
      if (logOutResult) {
        SweetAlerts.show({
          title: "Success",
          text: 'Signed out. Redirecting...'
        });
        window.location.reload();
      }
    }
  }
}

export default Header;
