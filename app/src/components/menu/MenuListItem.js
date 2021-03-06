import React from 'react';
import {View, Text, TouchableOpacity, Image} from 'react-native';
import {Icon}  from 'native-base';
import {style, StyleBase} from '../../styles/style';
import {Helper} from '../../common/constain';

export class MenuListItem extends React.Component{
  state = {};

  render() {
    return (
      <TouchableOpacity
        onPress={() => this.props.onItemClicked() }>
        <View style={[style.list_item, {backgroundColor: '#01589d'}]}>
          <View style={{flex:2, alignItems: 'flex-start', justifyContent: 'center'}}>
            {this.props.system ? (
                <Image style={[style.iconImgXs, {tintColor: '#fff'}]} source={this.props.itemIcon}/>
              ) : (
                <Image style={[style.iconImgXs, {tintColor: '#fff'}]} source={{uri: this.props.itemIcon || Helper.IconUrl}}/>
              )}
          </View>
          <View style={{flex:8, alignItems: 'flex-start', justifyContent: 'center'}}>
            <Text style={[this.props.actived ? {color: '#fff'} : {color: '#fff'}, {fontFamily: 'Source Sans Pro', fontSize: 16}]}>{this.props.itemText}</Text>
          </View>
        </View>
      </TouchableOpacity>
    )
  }
}