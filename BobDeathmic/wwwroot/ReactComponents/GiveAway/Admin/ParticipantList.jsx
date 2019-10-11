class ParticipantList extends React.Component {
    constructor(props) {
        super(props);
    }
    render() {
        let key = 0;
        let curthis = this;
        const participants = this.props.Participants.map(function (item) {
            key++;
            return (
                <li key={key}>
                    <h5>{item}{curthis.props.currentWinners.includes(item) && <i style={{color: "gold"}} className="fas fa-crown ml-2"/>}</h5>
                </li>
            );
        });
        return (
            <div>
                <h2>Teilnehmer</h2>
                <ol>
                    {participants}
                </ol>
            </div>
        );
    }
}
