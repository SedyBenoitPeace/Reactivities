import React, { useEffect, useState } from "react";
import { toast } from "react-toastify";
import { Button, Header, Icon, Segment } from "semantic-ui-react";
import agent from "../../app/api/agent";
import useQuery from "../../app/common/util/hooks";
import { useStore } from "../../app/stores/store";
import LoginForm from "./LoginForm";

export default function ConfirmEmail() {
    const { modalStore } = useStore();

    const email = useQuery().get('email') as string;
    const token = useQuery().get('token') as string;

    const Status = {
        Verifying: 'Verifying',
        Failed: 'Failed',
        Success: 'Success'
    };

    const [status, setStatus] = useState(Status.Verifying);

    function handleConfirmEmailResend() {
        agent.Account.resendEmailConfirmation(email).then(() => {
            toast.success('Verification email resent');
        }).catch(error => console.log(error));
    }

    useEffect(() => {
        agent.Account.verifyEmail(token, email).then(() => {
            setStatus(Status.Success)
        }).catch(() => {
            setStatus(Status.Failed);
        })
    }, [Status.Failed, Status.Success, token, email]);

    function getBody() {
        switch (status) {
            case Status.Verifying:
                return <p>Verifying....</p>
            case Status.Failed:
                return (
                    <div>
                        <p>Verification Failed, try to resend the email</p>
                        <Button primary onClick={handleConfirmEmailResend} content="Resend Email" size="huge"/>
                    </div>
                )
                case Status.Success:
                    return(
                        <div>
                        <p>Verification Succeeded no you can login</p>
                        <Button primary onClick={() => modalStore.openModal(<LoginForm />)} content="Login" size="huge"/>
                    </div>
                    )
        }
    }

    return (
        <Segment placeholder textAlign="center">
            <Header icon>
                <Icon name="envelope"/>
                Email verification
            </Header>
            <Segment.Inline>
                {getBody()}
            </Segment.Inline>
        </Segment>
    )
}