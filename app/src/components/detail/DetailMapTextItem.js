import React from 'react';
import {View, Image, TouchableOpacity, ScrollView, Text} from 'react-native';
import {Icon} from 'native-base';
import {style} from '../../styles/style';
import {AutoText} from '../../components/common/AutoText';
import {viewportHeight} from '../../common/constain';


export class DetailMapTextItem extends React.Component {
  render() {
    return (
      <View style={[style.detailItem,
      this.props.lastItem ? {} : {borderBottomWidth: 1, borderBottomColor: '#9f9f9f',}] }>
        <View style={{flex: 7, flexDirection: 'row'}}>
          <View style={{flex:15, justifyContent: 'center', alignItems: 'flex-start'}}>
            {/*<Icon name={d.infoIcon} style={{color:'#263238', fontSize:35}}/>*/}
            <Image style={style.iconImgXs} source={this.props.leftIcon}/>
          </View>
          <View style={{flex:85, justifyContent: 'center', alignItems: 'flex-start'}}>
            <AutoText height={((viewportHeight * 0.7) / 2)/4} numberOfLines={2}
                      style={[style.detailInfoText, ]}>{this.props.leftText || ''}</AutoText>
          </View>
        </View>
        <TouchableOpacity onPress={() => this.props.onMapItemClicked && this.props.onMapItemClicked()}
                          style={{flex: 3,flexDirection: 'row', }}>

          <View style={{flex:1, justifyContent: 'center', alignItems: 'flex-end'}}>
            <Text style={[style.detailInfoAction, ]}>{this.props.rightText || ''}</Text>
          </View>
        </TouchableOpacity>
      </View>
    );
  }
}