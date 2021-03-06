import React from 'react';
import {View, AsyncStorage} from 'react-native';
import {style} from '../../styles/style';
import {MenuTop} from './MenuTop';
import {MenuList} from './MenuList';
import {DNPageRoute, DNNavigationAction} from '../../NavigationHelper';
import {ListScreen} from '../../screens/ListScreen'
import {Language, Helper} from '../../common/constain';
import {PickerModal} from '../common/PickerModal';
import {IndustryListScreen} from '../../screens/IndustryListScreen';
import { NavigationActions } from 'react-navigation';
import {navigationStore, navigateToRouteAction} from '../../stores/NavigationStore';
import {appStore, appSaveLanguage} from '../../stores/AppStore'
import {changeAppLanguage} from '../../common/LocalizedStrings';
import {Menu} from '../../components/menu/Menu';
import {GDNServiceInstance} from '../../services/GDNService';

export class MenuContent extends React.Component {
  state = {selectedLang: 3, showPicker: false};

  async componentWillMount() {
    let langValue = await AsyncStorage.getItem(Helper.LanguageKey);
    this.setState({
      selectedLang: parseInt(langValue)
    });
  }

  render() {
    let props = {
      onListItemClicked: (id, isNoService, categoryName) => this.listItemClicked(id, isNoService, categoryName),
      onHelpClicked: () => this.helpClicked(),
      onAboutUsClicked: () => this.aboutUsClicked(),
      onLanguageClicked: () => this.languageUsClicked(),
      onFavoriteClicked: () => this.favoriteClicked(),
    };

    let language = Language.filter(l => l.Id == this.state.selectedLang)[0] || {};

    return (
      <View style={style.size1}>
        <View style={{flex: 10}}>
          <MenuTop onCloseMenu={() => this.props.onCloseMenu && this.props.onCloseMenu()}
                   onTakeCoverPhoto={()=> this.takeCoverPhoto()} onTakeAvatarPhoto={() => this.takeAvatarPhoto()}/>
        </View>
        <View style={{flex: 90}}>
          <MenuList {...props}/>
          <PickerModal tilte="Choose Language"
                       visible={this.state.showPicker}
                       onCloseModal={() => { this.setState({showPicker: false}) }}
                       selectedItem={language}
                       dataSource={Language}
                       onDataSelected={(d) => this.languageChanged(d) }/>
        </View>
      </View>

    )
  }

  takeCoverPhoto() {
  }

  takeAvatarPhoto() {
  }

  helpClicked() {

  }

  aboutUsClicked() {

  }

  listItemClicked(id, isNoService, categoryName) {
    if (isNoService) {
      let r = {listId: id, categoryName: categoryName};
      navigationStore.dispatch(navigateToRouteAction('IndustryListScreen',r));

    }
    else {
      let r = {listId: id, initIndex: 0, categoryName: categoryName};
      navigationStore.dispatch(navigateToRouteAction('ListScreen', r));

    }
    this.props.onCloseMenu && this.props.onCloseMenu();
  }

  languageUsClicked() {
    this.setState({showPicker: true});
    this.props.onCloseMenu && this.props.onCloseMenu();
  }

  async languageChanged(d) {

    this.setState({
      selectedLang: d.Id
    });
    changeAppLanguage(d.Id);
    await AsyncStorage.setItem(Helper.LanguageKey, d.Id + '');

    appStore.dispatch(appSaveLanguage(d.Id));

    let categoryId = await AsyncStorage.getItem(Helper.CurrentCategoryId);
    if(categoryId) {
      let title = await GDNServiceInstance.getCategoryNameById(categoryId);
      Menu.instance.setTitle(title);
    }
  }

  favoriteClicked() {
    this.props.onCloseMenu && this.props.onCloseMenu();
    navigationStore.dispatch(navigateToRouteAction('FavoriteScreen'));
  }
}