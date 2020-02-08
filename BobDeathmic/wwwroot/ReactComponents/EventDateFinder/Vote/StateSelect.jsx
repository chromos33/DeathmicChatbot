class StateSelect extends React.Component {
    constructor(props) {
        super(props);
        this.state = { possibleStates: props.possibleStates, State: props.state, RetryCount: 0,comment: props.comment};
        //this.handleOnChange = this.handleOnChange.bind(this);
        this.handleClick = this.handleClick.bind(this);
    }
    handleClick(event) {
        event.target.classList.add("processing");
        var thisreference = this;
        var tmpevent = event;
        var element = event.target;
        let value = tmpevent.target.getAttribute("data-value");
        let comment = "";
        if (value === "3") {
            comment = prompt("Kommentar eingeben", "");
        }
        thisreference.SyncStateToServer(thisreference.props.requestID, value, element,comment);
        
    }
    SyncStateToServer(ID, State, element,comment) {
        var thisreference = this;
        $.ajax({
            url: "/Events/UpdateRequestState/",
            type: "GET",
            data: {
                requestID: ID,
                state: State,
                comment: comment
            },
            success: function (result) {
                if (result > 0) {
                    thisreference.setState({
                        State: parseInt(element.getAttribute("data-value")),
                        comment: comment
                    });
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
                            if (tmpthis.props.mode === "default") {
                                return (<span className="voteoption" data-value="0" onClick={tmpthis.handleClick} key={tmpthis.props.key}>
                                    <i className="fas fa-minus" />
                                    <span className="lds-dual-ring" />
                                </span>);
                            }
                            else {
                                return (<a href={"/Events/UpdateRequestState/?fallback=true&requestID=" + tmpthis.props.requestID+"&state=0"} target="_blank" className="voteoption" data-value="0"  key={tmpthis.props.key}>
                                    <i className="fas fa-minus" />
                                    <span className="lds-dual-ring" />
                                </a>);
                            }
                            
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
                            if (tmpthis.props.mode === "default") {
                                return (<span className="voteoption greenbg" data-value="1" onClick={tmpthis.handleClick} onTouchEnd={tmpthis.handleClick} key={tmpthis.props.key}>
                                    <i className="fas fa-check" />
                                    <span className="lds-dual-ring" />
                                    </span>);
                            }
                            else {
                                return (<a href={"/Events/UpdateRequestState/?fallback=true&requestID=" + tmpthis.props.requestID + "&state=1"} target="_blank" className="voteoption greenbg" data-value="1" key={tmpthis.props.key}>
                                    <i className="fas fa-check" />
                                    <span className="lds-dual-ring" />
                                </a>);
                            }
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
                            if (tmpthis.props.mode === "default") {
                                return (<span className="voteoption redbg" data-value="2" onClick={tmpthis.handleClick} onTouchEnd={tmpthis.handleClick} key={tmpthis.props.key}>
                                    <i className="fas fa-times" />
                                    <span className="lds-dual-ring" />
                                    </span>);
                                }
                            else {
                                return (<a href={"/Events/UpdateRequestState/?fallback=true&requestID=" + tmpthis.props.requestID + "&state=2"} target="_blank" className="voteoption redbg" data-value="2" onClick={tmpthis.handleClick} onTouchEnd={tmpthis.handleClick} key={tmpthis.props.key}>
                                    <i className="fas fa-times" />
                                    <span className="lds-dual-ring" />
                                </a>);
                            }
                        }
                    }
                    if (state === "IfNeedBe") {
                        console.log(tmpthis.state);
                        if (tmpthis.state.State === 3) {
                            return (<span className="voteoption yellowbg active" data-value="3" key={tmpthis.props.key} onClick={tmpthis.handleClick} onTouchEnd={tmpthis.handleClick}>
                                <i className="fas fa-question" />
                                <span className="lds-dual-ring" />
                                {tmpthis.state.comment !== "" && tmpthis.state.comment !== undefined && tmpthis.state.comment !== null &&
                                    <i className="fas fa-info" />
                                }
                                {tmpthis.state.comment !== "" && tmpthis.state.comment !== undefined && tmpthis.state.comment !== null &&
                                    <span className="commentBox">{tmpthis.state.comment}</span>
                                }
                            </span>);
                        }
                        else {
                            if (tmpthis.props.mode === "default") {
                            return (<span className="voteoption yellowbg" data-value="3" onClick={tmpthis.handleClick} onTouchEnd={tmpthis.handleClick} key={tmpthis.props.key}>
                                <i className="fas fa-question" />
                                <span className="lds-dual-ring" />
                                </span>);
                            }
                            else {
                                return (<a href={"/Events/UpdateRequestState/?fallback=true&requestID=" + tmpthis.props.requestID + "&state=3"} target="_blank" className="voteoption yellowbg" data-value="3" onClick={tmpthis.handleClick} onTouchEnd={tmpthis.handleClick} key={tmpthis.props.key}>
                                    <i className="fas fa-question" />
                                    <span className="lds-dual-ring" />
                                </a>);
                            }
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
                                    {tmpthis.state.comment !== "" && tmpthis.state.comment !== undefined && tmpthis.state.comment !== null &&
                                        <i className="fas fa-info" />
                                    }
                                    {tmpthis.state.comment !== "" && tmpthis.state.comment !== undefined && tmpthis.state.comment !== null &&
                                        <span className="commentBox">{tmpthis.state.comment}</span>
                                    }
                                </span>
                            </div>
                        </span>
                    );
            }
        }
        
        return <p> No Users Loaded</p>;

    }
}