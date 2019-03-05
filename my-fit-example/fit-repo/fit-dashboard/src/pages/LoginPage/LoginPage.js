import * as React from 'react';
import { withRouter } from 'react-router-dom';
import { withStyles } from 'material-ui/styles';
import Button from 'material-ui/Button';
import Card, { CardHeader, CardContent } from 'material-ui/Card';
import Input from 'material-ui/Input';
import Typography from 'material-ui/Typography';

import styles from './LoginPage.styles';
import users from './users';
import AppHeader from '../../shared/AppHeader/AppHeader';
import LayoutGrid from 'material-ui/Grid';




class LoginPage extends React.Component {
    state = { username: '', password: '', failedLogin: false, activeAccounts: 0, accounts: [] };

    componentDidMount() {
        fetch('/admin/accounts')
            .then(res => res.json())
            .then(users => this.setState({ activeAccounts: users.length, accounts: users}));
    }
    
    updateState = ({ target }) => {
        this.setState({
            [target.name]: target.value,
            failedLogin: false
        });
    }

    login = (e) => {
        e.preventDefault();
        const user = users.find(user => user.username === this.state.username);
        if (!user) {
            this.setState({ password: '', failedLogin: true });
        } else {
            const homepage = {
                admin: '/',
                pm: `/pm/${user.costCode}`,
                eus: `/eus/${user.costCode}`,
                employee: `/employee/${user.username}`
            }[user.role];
            this.props.setUser(user);
            this.props.history.push(homepage);
        }
    }
    

    render() {
        const { classes } = this.props;
        const { username, password, failedLogin, activeAccounts, accounts } = this.state;
        
        return (
            
            <div classnames={classes.container}>
                <AppHeader />
                <form onSubmit={this.login} className={classes.form}>
                    <Card raised={true} className={classes.card}>
                        <CardHeader title={<Typography type="headline">Dashboard Login</Typography>} />
                        <CardContent>
                            
                            <Input
                                className={classes.input}
                                placeholder="Username"
                                name="username"
                                value={username}
                                onChange={this.updateState}
                                error={failedLogin}
                            />
                            <Input
                                className={classes.input}
                                placeholder="Password"
                                name="password"
                                type="password"
                                value={password}
                                onChange={this.updateState}
                                error={failedLogin}
                            />
                            <Button
                                className={classes.button}
                                raised={true}
                                disabled={!username || !password}
                                type="submit"
                                color="primary"
                            >
                                Login
                            </Button>
                            
                        </CardContent>
                        
                    </Card>
                    <ul>
            </ul>
                </form>
            </div>
        );
    }
}

export default withRouter(withStyles(styles)(LoginPage));
