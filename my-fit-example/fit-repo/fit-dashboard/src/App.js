import * as React from 'react';
import { BrowserRouter as Router, Switch, Route } from 'react-router-dom';
import ScrollToTop from './shared/ScrollToTop/ScrollToTop';
import PropsRoute from './shared/auth/PropsRoute';
import PrivateRoute from './shared/auth/PrivateRoute';
import LoginPage from './pages/LoginPage/LoginPage';
import Dashboard from './pages/Dashboard/Dashboard';
import filterRow from './pages/filterRow/filterRow';
import searchPage from './pages/searchPage/searchPage';
import expiring from './pages/ExpiringDevices/expiring';

class App extends React.Component {
    state = {
        user: undefined
    }
    
    setUser(user) {
        this.setState({ user });
    }
    

    render() {
        return (
            <Router onUpdate={() => window.scrollTo(0, 0)}>
                <ScrollToTop>
                    <Switch>
                    
                        <PropsRoute path="/login" component={LoginPage} setUser={this.setUser.bind(this)} />
                        <PrivateRoute path="/inventory" component={filterRow} user={this.state.user} />
                        <PrivateRoute path="/employee/:id" component={Dashboard} user={this.state.user} />
                        <PrivateRoute path="/eus/:id" component={Dashboard} user={this.state.user} />
                        <PrivateRoute path="/transactions" component={searchPage} user={this.state.user}/>
                        <PrivateRoute path="/expiring" component={expiring} user={this.state.user}/>
                        <PrivateRoute path="/" component={Dashboard} user={this.state.user} />
                        
                    </Switch>
                </ScrollToTop>
            </Router>
        )
    }
}

export default App;
