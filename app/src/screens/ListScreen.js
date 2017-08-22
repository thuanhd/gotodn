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
import {commonStore, updateCategoryName, CommonStoreActions} from '../stores/CommonStore';

export class ListScreen extends React.Component {
  state = {
    currentTab: 0,
    listData: [],
    scroll: false,
  };

  unSubscribe;
  unSubscribeCommon;

  componentWillMount() {
    navigationStore.subscribe(() => {
      let navigationState = navigationStore.getState();
      if (navigationState.routeName) {
        const navigateAction = NavigationActions.navigate({
          routeName: navigationState.routeName,
          params: navigationState.params
        });
        this.props.navigation.dispatch(navigateAction);
        if (navigationState.routeName == 'ListScreen' || navigationState.routeName == 'IndustryListScreen')
          if (typeof this.unSubscribeCommon === "function")
            this.unSubscribeCommon();
      }
    });
    this.unSubscribe = appStore.subscribe(() => {
      this.loadData();
    });

    this.unSubscribeCommon = commonStore.subscribe(() => {
      let commonState = commonStore.getState();
      if (commonState.type == CommonStoreActions.UpdateCategoryName) {
        let categories = commonState.categories;
        if (categories[categories.length - 1]) {
          Menu.instance.setTitle(categories[categories.length - 1].name);
        } else {
          Menu.instance.setTitle('');
        }
      }
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
    this.saveCategory(listId, data.categoryName);
    this.setState({
      listData: data,
    });
  }

  saveCategory(id, name) {
    let categories = commonStore.getState().categories || [];
    let categoryIndex = categories.map(c => {
      return c.id
    }).indexOf(id);
    if (categoryIndex !== -1) {
      categories[categoryIndex].name = name;
    } else {
      categories.push({id: id, name: name});
    }
    commonStore.dispatch(updateCategoryName(categories.slice()));
  }

  deleteCategory() {
    const {params} = this.props.navigation.state;
    let id = (params && params.listId) || 0;
    let categories = commonStore.getState().categories || [];
    let removeIndex = categories.map(c => {
      return c.id
    }).indexOf(id);
    if (removeIndex !== -1) {
      categories.splice(removeIndex, 1);
      commonStore.dispatch(updateCategoryName(categories.slice()));
    }
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
            renderTabBar={()=> <ScrollableTab />}
            style={{backgroundColor: '#29b6f6'}}
      >
        {this.state.listData.services.map((data, index) =>
          <Tab key={index}
               tabStyle={{backgroundColor:'#29b6f6',borderBottomWidth: 3, borderBottomColor:'#eeeeee'}}
               textStyle={{color:'#556c7a', fontWeight: 'normal'}}
               activeTabStyle={{backgroundColor: '#29b6f6',borderBottomWidth: 3, borderBottomColor:StyleBase.header_color}}
               activeTextStyle={{color:'#fff', fontWeight: 'normal'}}
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