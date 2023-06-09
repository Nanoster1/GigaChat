import React, {createContext, ReactNode} from "react";
import MobxState from "./MobxState";
import {observer} from "mobx-react-lite";

interface IStoreContext {
    mobxStore: MobxState
}

export const StoreContext = createContext<IStoreContext | null>(null)

export const StoreContextProvider = ({ children }: { children: ReactNode }) => {
    return (
        <StoreContext.Provider value={{ mobxStore: new MobxState() }}>
            {children}
        </StoreContext.Provider>
    );
}