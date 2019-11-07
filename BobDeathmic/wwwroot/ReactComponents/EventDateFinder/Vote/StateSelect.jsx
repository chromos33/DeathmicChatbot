class StateSelect extends React.Component {
    constructor(props) {
        super(props);
        this.state = { possibleStates: props.possibleStates, State: props.state, RetryCount: 0};
        //this.handleOnChange = this.handleOnChange.bind(this);
        this.handleClick = this.handleClick.bind(this);
    }
    handleClick(event) {
        event.target.classList.add("processing");
        var thisreference = this;
        var tmpevent = event;
        var element = event.target;
        thisreference.SyncStateToServer(thisreference.props.requestID, tmpevent.target.getAttribute("data-value"), element);
        
    }
    SyncStateToServer(ID, State, element) {
        var thisreference = this;
        $.ajax({
            url: "/Events/UpdateRequestState/",
            type: "GET",
            data: {
                requestID: ID,
                state: State
            },
            success: function (result) {
                if (result > 0) {
                    thisreference.setState({ State: parseInt(element.getAttribute("data-value")) });
                }
                else {
                    if (thisreference.state.RetryCount === 3) {
                        confirm("Einer deiner Abstimmungen will grade wohl nicht. Später nochmal probieren");
                        thisreference.setState({ RetryCount: 0 });
                    }
                    else {
                        var newcount = thisreference.state.RetryCount + 1;
                        thisreference.setState({ RetryCount: newcount });
                        thisreference.SyncStateToServer(ID, State, element);
                    }
                    
                    
                }
                element.classList.remove("processing");

            }
        });
    }
    /*handleOnChange(event) {
        this.setState({ State: event.target.value });
        var thisreference = this;
        var tmpevent = event;
        $.ajax({
            url: "/Events/UpdateRequestState/",
            type: "GET",
            data: {
                requestID: thisreference.props.requestID,
                state: event.target.value
            },
            success: function (result) {
            }
        });
    }*/
    render() {
        var tmpthis = this;
        if (this.props.canEdit) {
            
            if (this.state.possibleStates.length > 0) {
                var states = this.state.possibleStates.map(function (state) {
                    if (state === "NotYetVoted") {
                        if (tmpthis.state.State === 0) {
                            return (<span className="voteoption active" data-value="0" key={tmpthis.props.key}>
                                <i className="fas fa-minus" />
                            </span>);
                        }
                        else {
                            return (<span className="voteoption" data-value="0" onClick={tmpthis.handleClick} onTouchEnd={tmpthis.handleClick} key={tmpthis.props.key}>
                                <i className="fas fa-minus" />
                                <span className="lds-dual-ring" />
                            </span>);
                        }
                    }
                    if (state === "Available") {
                        
                        if (tmpthis.state.State === 1) {
                            return (<span className="voteoption greenbg active" data-value="1" key={tmpthis.props.key}>
                                <i className="fas fa-check" />
                                <span className="lds-dual-ring"/>
                            </span>);
                        }
                        else {
                            return (<span className="voteoption greenbg" data-value="1" onClick={tmpthis.handleClick} onTouchEnd={tmpthis.handleClick} key={tmpthis.props.key}>
                                <i className="fas fa-check" />
                                <span className="lds-dual-ring" />
                            </span>);
                        }
                    }
                    if (state === "NotAvailable") {
                        if (tmpthis.state.State === 2) {
                            return (<span className="voteoption redbg active" data-value="2" key={tmpthis.props.key}>
                                <i className="fas fa-times" />
                                <span className="lds-dual-ring" />
                            </span>);
                        }
                        else {
                            return (<span className="voteoption redbg" data-value="2" onClick={tmpthis.handleClick} onTouchEnd={tmpthis.handleClick} key={tmpthis.props.key}>
                                <i className="fas fa-times" />
                                <span className="lds-dual-ring" />
                            </span>);
                        }
                    }
                    if (state === "IfNeedBe") {
                        if (tmpthis.state.State === 3) {
                            return (<span className="voteoption yellowbg active" data-value="3" key={tmpthis.props.key}>
                                <i className="fas fa-question" />
                                <span className="lds-dual-ring" />
                            </span>);
                        }
                        else {
                            return (<span className="voteoption yellowbg" data-value="3" onClick={tmpthis.handleClick} onTouchEnd={tmpthis.handleClick} key={tmpthis.props.key}>
                                <i className="fas fa-question" />
                                <span className="lds-dual-ring" />
                            </span>);
                        }
                    }
                });
                return (
                    <span data-state={this.state.State} className="requestNode col-6 pt-0 pb-0 pr-0 pl-0">
                        <div className="d-flex">
                            {states}
                        </div>
                    </span>
                );
            }
        }
        else {
            switch (this.state.State) {
                case 0:
                    return (
                        <span className="requestNode col-6 pt-0 pb-0 pr-0 pl-0" data-state={this.state.State}>
                            <div className="d-flex">
                                <span className="voteoption voteoptionforeign" key={tmpthis.props.key}>
                                    <i className="fas fa-minus" />
                                </span>
                            </div>
                        </span>
                    );
                case 1:
                    return (
                        <span className="requestNode col-6 pt-0 pb-0 pr-0 pl-0" data-state={this.state.State}>
                            <div className="d-flex">
                                <span className="voteoption greenbg voteoptionforeign" key={tmpthis.props.key}>
                                    <i className="fas fa-check" />
                                </span>
                            </div>
                        </span>
                    );
                case 2:
                    return (
                        <span className="requestNode col-6 pt-0 pb-0 pr-0 pl-0" data-state={this.state.State}>
                            <div className="d-flex">
                                <span className="voteoption redbg voteoptionforeign" key={tmpthis.props.key}>
                                    <i className="fas fa-times" />
                                </span>
                            </div>
                        </span>
                    );
                case 3:
                    return (
                        <span className="requestNode col-6 pt-0 pb-0 pr-0 pl-0" data-state={this.state.State}>
                            <div className="d-flex">
                                <span className="voteoption yellowbg voteoptionforeign" key={tmpthis.props.key}>
                                    <i className="fas fa-question" />
                                </span>
                            </div>
                        </span>
                    );
            }
        }
        
        return <p> No Users Loaded</p>;

    }
}