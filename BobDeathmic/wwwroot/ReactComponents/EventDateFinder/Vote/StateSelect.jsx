class StateSelect extends React.Component {
    constructor(props) {
        super(props);
        this.state = { possibleStates: props.possibleStates, State: props.state};
        this.handleOnChange = this.handleOnChange.bind(this);
    }
    handleOnChange(event) {
        this.setState({ State: event.target.value });
        var thisreference = this;
        var tmpevent = event;
        $.ajax({
            url: "/EventDateFinder/UpdateRequestState/",
            type: "GET",
            data: {
                requestID: thisreference.props.requestID,
                state: event.target.value
            },
            success: function (result) {
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
                            return <option value="0">Noch nicht entschieden</option>;
                        }
                        else {
                            return <option value="0">Noch nicht entschieden</option>;
                        }
                    }
                    if (state === "Available") {
                        if (tmpthis.state.State === 1) {
                            return <option value="1">Ich kann</option>;
                        }
                        else {
                            return <option value="1">Ich kann</option>;
                        }
                    }
                    if (state === "NotAvailable") {
                        if (tmpthis.state.State === 2) {
                            return <option value="2">Ich kann nicht</option>;
                        }
                        else {
                            return <option value="2">Ich kann nicht</option>;
                        }
                    }
                    if (state === "IfNeedBe") {
                        if (tmpthis.state.State === 3) {
                            return <option value="3">Wenn es sein muss</option>;
                        }
                        else {
                            return <option value="3">Wenn es sein muss</option>;
                        }
                    }
                });
                return (
                    <div>
                        <select key={this.props.key} value={this.state.State} onChange={this.handleOnChange} className={"chatUser_" + this.props.key}>
                            {states}
                        </select>
                    </div>
                );
            }
        }
        else {
            switch (this.state.State) {
                case 0:
                    return (
                        <div>
                            <p className="mb-0">
                                Noch nicht entschieden
                            </p>
                        </div>
                    );
                case 1:
                    return (
                        <div>
                            <p className="mb-0">
                                Ich kann
                            </p>
                        </div>
                    );
                case 2:
                    return (
                        <div>
                            <p className="mb-0">
                                Ich kann nicht
                            </p>
                        </div>
                    );
                case 3:
                    return (
                        <div>
                            <p className="mb-0">
                                Wenn es sein muss
                            </p>
                        </div>
                    );
            }
        }
        
        return <p> No Users Loaded</p>;

    }
}