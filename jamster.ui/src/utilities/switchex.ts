class SwitchNoResult<TValue> {
    private _value: TValue;
    
    constructor(value: TValue) {
        this._value = value;
    }

    public case(caseValue: TValue): CaseNoResult<TValue> {
        return this.predicate(v => v === caseValue);
    }

    public if(value: boolean): CaseNoResult<TValue> {
        return this.predicate(_ => value);
    }

    public predicate(predicate: (value: TValue) => boolean): CaseNoResult<TValue> {
        if(predicate(this._value)) {
            return new CaseTrueNoResult(this._value);
        } else {
            return new CaseFalseNoResult(this._value);
        }
    }

    public default<TResult>(result: TResult): TResult {
        return result;
    }
}

type Switch<TValue, TResult> = {
    case(caseValue: TValue): Case<TValue, TResult>;
    if(value: boolean): Case<TValue, TResult>;
    predicate(predicate: (value: TValue) => boolean): Case<TValue, TResult>;
    default<TDefaultResult>(result: TDefaultResult): TResult | TDefaultResult;
}

class SwitchUnresolved<TValue, TResult> implements Switch<TValue, TResult> {
    private _value: TValue;

    constructor(value: TValue) {
        this._value = value;
    }

    public case(caseValue: TValue): Case<TValue, TResult> {
        return this.predicate(v => v === caseValue);
    }

    public if(value: boolean): Case<TValue, TResult> {
        return this.predicate(_ => value);
    }

    public predicate(predicate: (value: TValue) => boolean): Case<TValue, TResult> {
        if (predicate(this._value)) {
            return new CaseTrue<TValue, TResult>(this._value);
        } else {
            return new CaseFalse<TValue, TResult>(this._value);
        }
    }

    default<TDefaultResult>(result: TDefaultResult): TDefaultResult {
        return result;
    }
}

class SwitchResolved<TValue, TResult> implements Switch<TValue, TResult> {
    private _result: TResult;

    constructor(result: TResult) {
        this._result = result;
    }

    public case(_: TValue): Case<TValue, TResult> {
        return new CaseResolved<TValue, TResult>(this._result);
    }

    public if(value: boolean): Case<TValue, TResult> {
        return this.predicate(_ => value);
    }

    public predicate(_: (value: TValue) => boolean): Case<TValue, TResult> {
        return new CaseResolved<TValue, TResult>(this._result);
    }

    public default(_: never): TResult {
        return this._result;
    }


}

type CaseNoResult<TValue> = {
    when(predicate: () => boolean): CaseNoResult<TValue>;
    then<TThenResult>(result: TThenResult): Switch<TValue, TThenResult>;
}

class CaseTrueNoResult<TValue> implements CaseNoResult<TValue> {
    private _value: TValue;

    constructor(value: TValue) {
        this._value = value;
    }

    when(predicate: () => boolean): CaseNoResult<TValue> {
        if(predicate()) {
            return this;
        } else {
            return new CaseFalseNoResult(this._value);
        }
    }

    then<TThenResult>(result: TThenResult): Switch<TValue, TThenResult> {
        return new SwitchResolved<TValue, TThenResult>(result);
    }

}

class CaseFalseNoResult<TValue> implements CaseNoResult<TValue> {
    private _value: TValue;
    
    constructor(value: TValue) {
        this._value = value;
    }

    when(_: () => boolean): CaseNoResult<TValue> {
        return this;
    }

    then<TThenResult>(_: TThenResult): Switch<TValue, TThenResult> {
        return new SwitchUnresolved<TValue, TThenResult>(this._value);
    }


}

type Case<TValue, TResult> = {
    when(predicate: () => boolean): Case<TValue, TResult>;
    then<TThenResult extends TResult>(result: TThenResult): Switch<TValue, TResult | TThenResult>;
}

class CaseResolved<TValue, TResult> implements Case<TValue, TResult> {
    private _result: TResult;

    constructor(result: TResult) {
        this._result = result;
    }

    public when(_: () => boolean): Case<TValue, TResult> {
        return this;
    }

    public then<TThenResult extends TResult>(_: TThenResult): Switch<TValue, TResult | TThenResult> {
        return new SwitchResolved<TValue, TResult | TThenResult>(this._result);
    }
}

class CaseTrue<TValue, TResult> implements Case<TValue, TResult> {
    private _value: TValue;

    constructor(value: TValue) {
        this._value = value;
    }

    public when(predicate: () => boolean): Case<TValue, TResult> {
        if(predicate()) {
            return this;
        } else {
            return new CaseFalse(this._value);
        }
    }

    public then<TThenResult extends TResult>(result: TThenResult): Switch<TValue, TResult | TThenResult> {
        return new SwitchResolved<TValue, TResult | TThenResult>(result);
    }
}

class CaseFalse<TValue, TResult> implements Case<TValue, TResult> {
    private _value: TValue;

    constructor(value: TValue) {
        this._value = value;
    }

    public when(_: () => boolean): Case<TValue, TResult> {
        return this;
    }

    public then<TThenResult extends TResult>(_: TThenResult) {
        return new SwitchUnresolved<TValue, TResult | TThenResult>(this._value);
    }
}

export const switchex = <TValue>(value: TValue) => {
    return new SwitchNoResult(value);
}

export const ternary = () => {
    return new SwitchNoResult<object>({});
}