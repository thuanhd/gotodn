import React from 'react';
import {Text, View, Image, ScrollView, TouchableOpacity, Dimensions, RefreshControl} from 'react-native';
import {Icon, Spinner} from 'native-base';
import {StyleBase} from '../../styles/style';
import {style} from "../../styles/style";
import {Helper} from '../../common/constain';
import {DNPageRoute} from '../../NavigationHelper';
import {DetailScreen} from '../../screens/DetailScreen';
import {navigationStore, navigateToRouteAction} from '../../stores/NavigationStore';
import {
  LazyloadImage,
  LazyloadView
} from 'react-native-lazyload';
import {LStrings} from '../../common/LocalizedStrings';

const {width: viewportWidth, height: viewportHeight} = Dimensions.get('window');
const largeImgHeight = Math.round(viewportHeight / 3);
const smallImgHeight = Math.round(viewportHeight / 5);
const textHeight = Math.round(viewportHeight / 6.5);
const largeItemHeight = largeImgHeight + textHeight;
const smallItemHeight = smallImgHeight + textHeight;

export class ListItemDetail extends React.Component {
  state = {
    isLoaded: false,
    dataLeft: [],
    dataRight: [],
    refreshing: false,
  };

  componentWillReceiveProps(nextProps) {
    this.loadData(nextProps.dataSource);
  }

  componentDidMount() {
  }

  loadData(listData) {
    if (listData) {
      let dataLeft = [], dataRight = [];
      for (let i = 0; i < listData.length; i++) {
        let data = listData[i];
        if (i % 2 === 0) dataLeft.push(data);
        else dataRight.push(data);
      }
      this.setState({
        dataLeft: dataLeft,
        dataRight: dataRight,
        isLoaded: true,
        refreshing: false,
      });
    } else {
      setTimeout(() => {
        this.setState({
          isLoaded: true,
          refreshing: false,
        });
      }, 1000);
    }
  }

  renderStart(starScore) {

    let floorStar = Math.floor(starScore);
    let modStar = starScore - floorStar;
    let stars = [];
    for (let i = 0; i < 5; i++) {
      if (i < floorStar)
        stars.push(1);
      else {
        if (modStar > 0) {
          if (modStar <= 0.5) {
            stars.push(0);
          } else {
            stars.push(1);
          }
          modStar = 0;
        } else
          stars.push(0);
      }
    }
    return (
      <View style={style.menuItemStar}>
        {stars.map((t, key) => t == 1 ?
          <Icon key={key}
                style={{alignSelf: 'flex-end',color: '#fced00', fontSize:20,width: 20, paddingHorizontal: 1}}
                name='ios-star'/> :
          <Icon key={key}
                style={{alignSelf: 'flex-end',color: '#fced00', fontSize:20,width: 20, paddingHorizontal: 1}}
                name='ios-star-outline'/>)}
      </View>
    );
  }


  render() {
    return (
      !this.state.isLoaded ?
        (<View style={{flex:1, alignItems: 'center', justifyContent: 'center',
                backgroundColor: '#ffffff',minHeight: Math.round(viewportHeight*5/10), paddingTop: 10}}>
          <View style={{alignSelf:'center'}}>
            <Spinner color={StyleBase.header_color}/>
          </View>
        </View>)
        :
        (
          <View style={[style.container, {paddingTop: 10}]}
                onLayout={(event) => {
                    const {height} = event.nativeEvent.layout;
                    let left = this.state.dataLeft ? this.state.dataLeft.length : 0;
                    let right = this.state.dataRight ? this.state.dataRight.length : 0;
                    let itemHeight =  0;
                    if(left != 0 || right != 0) {
                      itemHeight =  height / (left != 0 && right != 0 ? (left > right ? left : right) : (left != 0 ? left : right));
                    }
                    this.props.onLayoutItem && this.props.onLayoutItem(itemHeight);
                }}
          >
            <View style={[style.containerHalf, {marginLeft:10, marginRight:5}]}>
              {this.state.dataLeft.map((data, index) =>
                <View key={index} style={{alignSelf: 'stretch'}}>
                  <TouchableOpacity
                    activeOpacity={0.7}
                    style={[style.menuItemDetail]}
                    onPress={() => {this.goToDetail(data.id)}}
                    key={index}
                  >
                    <View style={style.imageContainer}>
                      <View>
                        <LazyloadImage
                          host="lazyload-listscreen"
                          source={{uri: data.heroImage || Helper.ImageUrl}}
                          style={[style.menuItemImage, {height: (index % 2 === 0) ? largeImgHeight : smallImgHeight}]}
                        >
                          {data.star > 0 && this.renderStart(data.star)}
                        </LazyloadImage>
                      </View>
                      <LazyloadView
                        host="lazyload-listscreen"
                        style={{flex: 1}}>
                        <View style={style.menuItemTextContain}>
                          <Text style={style.menuItemTitle} numberOfLines={1}>{ data.title || LStrings.NoTitle }</Text>
                          <Text style={[style.menuItemSubTitle, !data.description ? {lineHeight: 24} : null]} numberOfLines={4}>{ data.description || LStrings.NoDescription }</Text>
                        </View>
                      </LazyloadView>
                    </View>
                  </TouchableOpacity>
                </View>
              )}
            </View>
            <View style={[style.containerHalf, {marginLeft:5, marginRight:10}]}>
              {this.state.dataRight.length > 0 ? this.state.dataRight.map((data, index) =>
                  <View key={index} style={{alignSelf: 'stretch'}}>
                    <TouchableOpacity
                      activeOpacity={0.7}
                      style={[style.menuItemDetail]}
                      onPress={() => {this.goToDetail(data.id)}}
                      key={index}
                    >
                      <View style={style.imageContainer}>
                        <View>
                          <LazyloadImage
                            host="lazyload-listscreen"
                            source={{uri: data.heroImage || Helper.ImageUrl}}
                            style={[style.menuItemImage, {height: (index % 2 !== 0) ? largeImgHeight : smallImgHeight}]}
                          >
                            {data.star > 0 && this.renderStart(data.star)}
                          </LazyloadImage>
                        </View>
                        <LazyloadView
                          host="lazyload-listscreen"
                          style={{flex: 1}}>
                          <View style={style.menuItemTextContain}>
                            <Text style={style.menuItemTitle} numberOfLines={2}>{ data.title || LStrings.NoTitle }</Text>
                            <Text style={[style.menuItemSubTitle, !data.description ? {lineHeight: 24} : null]} numberOfLines={4}>{ data.description || LStrings.NoDescription }</Text>
                          </View>
                        </LazyloadView>
                      </View>
                    </TouchableOpacity>
                  </View>
                ) : (<View style={{alignSelf: 'stretch'}}/>)}
            </View>
          </View>
        ))
  }

  onFresh() {
    this.setState({refreshing: true});
    this.loadData(this.props.dataSource);
  }

  goToDetail(id) {
    navigationStore.dispatch(navigateToRouteAction('DetailScreen', {itemId: id}));
  }
}