import { EventWithBody } from "./Event";
import { Ruleset } from "../Ruleset";

export class RulesetSet extends EventWithBody {
    constructor(rules: Ruleset) {
        super("RulesetSet", { rules });
    }
}