import {Platform, Dimensions} from 'react-native';

export const {width: viewportWidth, height: viewportHeight} = Dimensions.get('window');
export const platform = Platform.OS;