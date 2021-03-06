import React from 'react';
import {PanResponder} from 'react-native';
import {Tabs, Tab, Left, Thumbnail, Body, Button, Icon, TabHeading, Text, ScrollableTab} from 'native-base';
import {viewportWidth, MenuListData, MenuType, Helper} from '../common/constain';
import {ListDetail} from '../components/list/ListDetail';
import {Menu} from '../components/menu/Menu';
import {StyleBase} from '../styles/style';
import {NavigationActions} from 'react-navigation';
import {navigationStore, navigateAction} from '../stores/NavigationStore';
import {GDNServiceInstance} from '../services/GDNService';
import {appStore} from '../stores/AppStore';

export class ListScreen extends React.Component {
  state = {
    currentTab: 0,
    listData: [],
    scroll: false,
  };

  unSubscribe;
  unSubscribeCommon;

  componentWillMount() {
    this.unSubscribe = appStore.subscribe(() => {
      this.loadData();
    });
  }

  componentDidMount() {
    this.loadData();
  }

  async componentWillUnmount() {
    if (typeof this.unSubscribe === "function")
      this.unSubscribe();
    if (typeof this.unSubscribeCommon === "function")
      this.unSubscribeCommon();
  }

  async loadData() {
    const {params} = this.props.navigation.state;
    let listId = (params && params.listId) || 0;
    let data = await GDNServiceInstance.getCategoryById(listId);
    this.setState({
      listData: data,
    });
  }

  renderTabBar() {
    return (
      <ScrollableTab
        tabsContainerStyle={{justifyContent:'center'}}
        tabStyle={{backgroundColor:'#29b6f6',borderBottomWidth: 3, borderBottomColor:'#eeeeee'}}
        textStyle={{color:'#556c7a', fontWeight: 'normal'}}
        activeTabStyle={{backgroundColor: '#29b6f6'}}
        activeTextStyle={{color:'#fff', fontWeight: 'normal'}}
        underlineStyle={{backgroundColor: StyleBase.header_color}}
      />
    )
  }

  render() {
    // let numberOfTab = this.state.listData.length < 3 ? this.state.listData.length : 3;
    //
    // let tabWidth = viewportWidth / (numberOfTab == 0 ? 1 : numberOfTab);
    // let curTab = this.state.currentTab;

    const {params} = this.props.navigation.state;
    let listId = (params && params.listId) || 0;
    let curTab = params && params.initIndex;
    if (!this.state.listData || !this.state.listData.services) return null;
    return (
      <Tabs initialPage={curTab || 0}
            locked
            onChangeTab={(page) => this.tabChanged(page)}
            renderTabBar={()=> this.renderTabBar()}
            style={{backgroundColor: '#29b6f6'}}
      >
        {this.state.listData.services.map((data, index) =>
          <Tab key={index}
               tabStyle={{backgroundColor:'#29b6f6',borderBottomWidth: 3, borderBottomColor:'#eeeeee', minWidth: viewportWidth/(this.state.listData.services.length || 1)}}
               textStyle={{color:'#556c7a', fontWeight: 'normal'}}
               activeTabStyle={{backgroundColor: '#29b6f6', minWidth: viewportWidth/(this.state.listData.services.length || 1)}}
               activeTextStyle={{color:'#fff', fontWeight: 'normal'}}
               underlineStyle={{backgroundColor: StyleBase.header_color}}
               heading={data.title}>
            <ListDetail serviceId={data.id}
            />
          </Tab>
        )}
      </Tabs>

    )
  }

  tabChanged(page) {
    this.setState({
      currentTab: page.i
    });
  }
}