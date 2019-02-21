import * as React from 'react';
import { withRouter } from 'react-router-dom';
import { withStyles } from 'material-ui/styles';
import logo from './logo.svg';
import AppBar from 'material-ui/AppBar';
import Toolbar from 'material-ui/Toolbar';
import Typography from 'material-ui/Typography';
import Button from 'material-ui/Button';

const styles = (theme) => ({
    root: {
        width: '100%'
    },
    appBar: {
        backgroundColor: '#607D8B',
        padding: '0 4rem',
        textTransform: 'uppercase'
    },
    logo: {
        height: '3rem',
        padding: '1rem 0'
    },
    title: {
        paddingLeft: '2rem'
    },
    navLinks: {
        paddingLeft: '1rem'
    },
    caption: {
        marginLeft: 'auto'
    },
    pageTitle: {
        paddingLeft: '6rem'
    }
});

const Header = ({ classes, title, history }) => (
    <div className={classes.root}>
        <AppBar position="static" className={classes.appBar}>
            <Toolbar>
                <img src={logo} className={classes.logo} alt="FIT Logo" />
                <Typography type="title" color="inherit" className={classes.title}>
                    Physical Asset Manager
                </Typography>
                {title && // TODO: actually check for logged in user, not just title
                <div className={classes.navLinks}>
                    <Button color="inherit" onClick={() => history.push('/')}>Home</Button>
                    <Button color="inherit" onClick={() => history.push('/inventory')}>Inventory</Button>
                    <Button color="inherit" onClick={() => history.push('/transactions')}>Transactions</Button>
                    <Button color="inherit" onClick={() => history.push('/expiring')}>Expiring Devices</Button>
                </div>}
                <Typography type="caption" color="inherit" align="right" className={classes.caption}>
                    Bureau of the Fiscal Service
                    <br />
                    U.S. Department of the Treasury
                </Typography>
            </Toolbar>
        </AppBar>
        <h1 type="display1" className={classes.pageTitle}>{title}</h1>
    </div>
);

export default withStyles(styles)(withRouter(Header));
