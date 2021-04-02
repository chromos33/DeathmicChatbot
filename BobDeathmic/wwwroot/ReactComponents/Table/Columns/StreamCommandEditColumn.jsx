class StreamCommandEditColumn extends React.Component {
    constructor(props) {
        super(props);
        this.handleTypeClick = this.handleTypeClick.bind(this);
        this.getTypeSwitchCSSClasses = this.getTypeSwitchCSSClasses.bind(this);
        this.handleNameChange = this.handleNameChange.bind(this);
        this.handleUptimeChange = this.handleUptimeChange.bind(this);
        this.handleQuoteChange = this.handleQuoteChange.bind(this);
        this.channelswitch = this.channelswitch.bind(this);
        this.handleSave = this.handleSave.bind(this);
        this.handleLoadEditForm = this.handleLoadEditForm.bind(this);
        this.handleCancel = this.handleCancel.bind(this);
        this.responsechange = this.responsechange.bind(this);
        this.state = {
            Open: false,
            CanSave: true,
            Name: "",
            Response: "",
            Mode: "",
            StreamID: 0,
            StreamName: "",
            Streams: [],
            Change: false
        };
    }
    handleTypeClick(e) {
        this.setState({
            Mode: e.target.getAttribute("data-type"),
            Change: true
        });
    }
    responsechange(e) {
        if (e.target.value === "") {
            this.setState({
                Response: e.target.value,
                CanSave: false,
                Change: true
            });
        }
        else {
            this.setState({
                Response: e.target.value,
                CanSave: true,
                Change: true
            });
        }
    }
    handleNameChange(e) {
        if (e.target.value === "") {
            this.setState({
                Name: e.target.value,
                CanSave: false,
                Change: true
            });
        }
        else {
            this.setState({
                Name: e.target.value,
                CanSave: true,
                Change: true
            });
        }

    }
    channelswitch(e) {
        let index = e.nativeEvent.target.selectedIndex;
        this.setState({
            StreamID: this.state.Streams[index].ID,
            StreamName: this.state.Streams[index].Name,
            Change: true
        });
    }
    handleUptimeChange(e) {
        this.setState({
            AutoInterval: e.target.value,
            Change: true
        });
    }
    handleQuoteChange(e) {
        this.setState({
            Quote: e.target.value,
            Change: true
        });
    }
    getTypeSwitchCSSClasses(type) {
        let classes = "typeswitch";
        if (type === this.state.Mode) {
            classes += " active";
        }
        classes += " " + type;
        return classes;
    }
    handleSave(e) {
        if (this.state.CanSave && this.state.Change) {
            var request = new XMLHttpRequest();
            request.open("POST", "/StreamCommands/SaveCommand", true);
            var curthis = this;
            request.setRequestHeader("Content-Type", "application/x-www-form-urlencoded");
            request.onreadystatechange = function () { // Call a function when the state changes.
                if (this.readyState === XMLHttpRequest.DONE && this.status === 200) {
                    curthis.setState({ Open: false, Change: false });
                    window.dispatchEvent(new Event('updateTable'));
                }
            }
            let streamID = this.state.StreamID;
            if (streamID === 0) {
                streamID = this.state.Streams[0].ID
            }
            request.send(
                "ID=" + this.props.data.CommandID +
                "&Name=" + this.state.Name +
                "&Response=" + this.state.Response +
                "&Mode=" + this.state.Mode +
                "&StreamID=" + streamID + 
                "&AutoInterval="+ this.state.AutoInterval
            );
        }
        else {
            this.setState({ Open: false });
        }
    }

    handleCancel(e) {
        this.setState({ Open: false });
    }
    handleLoadEditForm(e) {
        const xhr = new XMLHttpRequest();
        var thisreference = this;
        console.log(this.props);
        xhr.open('GET', "/StreamCommands/GetEditData?streamCommandID=" + this.props.data.CommandID, true);
        xhr.onload = function () {
            if (xhr.responseText !== "") {
                let data = JSON.parse(xhr.responseText);
                console.log(data);
                
                thisreference.setState({
                    Open: true,
                    Name: data.Name,
                    Response: data.Response,
                    Mode: data.Mode,
                    StreamID: data.StreamID,
                    StreamName: data.StreamName,
                    Streams: data.Streams,
                    AutoInterval: data.AutoInterval
                });
            }
        };
        xhr.send();
    }
    render() {
        if (this.state.Open) {
            const options = this.state.Streams.map((e) => {
                return <option value={e.ID}>{e.Name}</option>
            })
            return <td>
                {this.props.data.Text}
                <div className="shadowlayer"></div>
                <div className="statictest grid">
                    <label className="namelabel">Name</label>
                    <input className="namefield" name="Name" value={this.state.Name} onChange={this.handleNameChange} type="text" />
                    <label className="typelabel">Type</label>
                    <span data-type="Manual" onClick={this.handleTypeClick} className={this.getTypeSwitchCSSClasses("Manual")}>Manual</span>
                    <span data-type="Auto" onClick={this.handleTypeClick} className={this.getTypeSwitchCSSClasses("Auto")}>Auto</span>
                    <span data-type="Random" onClick={this.handleTypeClick} className={this.getTypeSwitchCSSClasses("Random")}>Random</span>
                    {this.state.Mode === "Auto" &&
                    <div className="autointerval">
                        <label className="intervallabel">Auto Interval</label>
                        <input value={this.state.AutoInterval} onChange={this.handleUptimeChange} className="intervalfield" type="number" />
                    </div>
                        }
                    <label className="streamlabel">Stream</label>
                    <select className="streamselect" value={this.state.StreamID} onChange={this.channelswitch}>{options}</select>
                    <label className="responselabel">Text</label>
                    <textarea className="responsefield" value={this.state.Response} onChange={this.responsechange} />
                    <span onClick={this.handleSave} className="btn btn_primary savebtn">Speichern</span>
                    <span onClick={this.handleCancel} className="btn btn_primary cancelbtn">Abbrechen</span>
                </div>
            </td>;
        }
        else {
            return <td className="pointer" onClick={this.handleLoadEditForm}>
                {this.props.data.Text}
            </td>;
        }


    }
}
