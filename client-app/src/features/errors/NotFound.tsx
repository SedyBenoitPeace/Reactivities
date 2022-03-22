import React from "react";
import { Link } from "react-router-dom";
import { Button, Header, Icon, Segment } from "semantic-ui-react";

export default function NotFound() {
    return (
        <Segment>
            <Header icon>
                <Icon name='search'></Icon>
                Oop - we've looke everywhere and we could not find this
            </Header>
            <Segment.Inline>
                <Button as={Link} to='/activities' primary></Button>
            </Segment.Inline>
        </Segment>
    )
}