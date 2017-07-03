import * as React from 'react';
import {Alert, StyleSheet, TouchableOpacity, View} from 'react-native';
import {Icon} from 'native-base';
import {viewportWidth, viewportHeight} from '../../common/constain';
const MapView = require('react-native-maps');
const {PROVIDER_GOOGLE} = MapView;

interface thisProps {
}

interface thisState {
}

const GPS_ZOOM = .01;

export class ReactMap extends React.Component<thisProps, thisState> {
  state = {
    currentPos: {
      latitude: 16.047515,
      longitude: 108.17122,
      latitudeDelta: 0.3,
      longitudeDelta: 0.3 * (viewportWidth / viewportHeight),
    },
  };

  componentWillMount() {
  }

  componentDidMount() {

    this.getCurrentPos();
  }

  getCurrentPos() {
    navigator.geolocation.getCurrentPosition((position) => {

        this.animateRegion(position);

      }, (error) => this.setState({initError: error.message}),
      {enableHighAccuracy: true, timeout: 20000, maximumAge: 1000});
  }

  animateRegion(position) {
    let region = {
      latitude: position.coords.latitude - .003,
      longitude: position.coords.longitude,
      latitudeDelta: GPS_ZOOM,
      longitudeDelta: GPS_ZOOM * (viewportWidth / viewportHeight)
    };
    this.mapRef.animateToRegion(region, 500);
    let currentPos = this.state.current;
    currentPos.latitude = position.coords.latitude;
    currentPos.longitude = position.coords.longitude;
    currentPos.latitudeDelta = GPS_ZOOM;
    currentPos.longitudeDelta = GPS_ZOOM * (viewportWidth / viewportHeight);
    this.setState({currentPos: currentPos});
  }

  render() {

    return (
      <View style={{flex:1}}>
        <MapView
          {...this.props}
          style={MapStyle.map}
          ref={(ref: any) => {
          this.mapRef = ref;
        }}
          provider={PROVIDER_GOOGLE}
          loadingEnabled
          loadingIndicatorColor='#666666'
          loadingBackgroundColor='#eeeeee'
          showsUserLocation
        >
          {/*<MapView.Marker*/}
          {/*coordinate={this.props.region}*/}
          {/*/>*/}

        </MapView>
        <View style={{position: 'absolute', left: 5, top: 5,
        backgroundColor: 'rgba(255,255,255,.8)',
        alignItems: 'center', justifyContent: 'center',
        width:34, height: 34, borderRadius: 17
         }}>
          <TouchableOpacity onPress={() => this.getCurrentPos()}>
            <Icon name={'ios-locate-outline'} style={{color:'#039be5', fontSize:30}}/>
          </TouchableOpacity>
        </View>
      </View>
    );
  }
}

const MapStyle = StyleSheet.create({
  map: {
    ...StyleSheet.absoluteFillObject,
  }
});