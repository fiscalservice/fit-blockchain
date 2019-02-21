import * as React from 'react';
import { Redirect, Route } from 'react-router-dom';
import { renderMergedProps } from './PropsRoute';

export default ({ component, redirectTo = '/login', user, ...rest }) => (
    <Route {...rest} render={props => user
        ? renderMergedProps(component, props, { user: user }, rest)
        : <Redirect to={{
            pathname: redirectTo,
            state: { from : props.location}
        }} />
    } />
);
