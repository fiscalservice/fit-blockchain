import * as React from 'react';
import { Route } from 'react-router-dom';

export const renderMergedProps = (component, ...rest) => {
    const props = Object.assign({}, ...rest);
    return React.createElement(component, props);
};

export default ({ component, ...rest }) => (
    <Route {...rest} render={props => renderMergedProps(component, props, rest)} />
);
