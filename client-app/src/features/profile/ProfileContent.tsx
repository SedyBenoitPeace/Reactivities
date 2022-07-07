import { observer } from 'mobx-react-lite';
import React from 'react';
import { Tab } from 'semantic-ui-react';
import { Profile } from '../../app/models/profile';
import ProfilePhotos from './ProfilePhotos';

interface Props {
    profile: Profile;
}

export default observer (function ProfileContent({profile}: Props){
    const panes = [
        {menuItem: 'About', render: () => <Tab.Pane> About Content</Tab.Pane>},
        {menuItem: 'Photos', render: () => <ProfilePhotos profile={profile}/>},
        {menuItem: 'Events', render: () => <Tab.Pane> About Events</Tab.Pane>},
        {menuItem: 'Followers', render: () => <Tab.Pane> About Followers</Tab.Pane>},
        {menuItem: 'Following', render: () => <Tab.Pane> About Following</Tab.Pane>},
    ];

    return (
        <Tab
            menu={{fluid: true, vertical: true}}
            menuPosition='right'
            panes={panes}
        />
    )
})