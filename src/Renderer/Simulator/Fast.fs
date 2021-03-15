﻿module Fast
    open Fable.Core
    open CommonTypes
    open SimulatorTypes


    type FastData =
        | Bit of uint32 // must be 0 or 1, allows bitwise logical operators
        | Word of dat:uint32 * width:int
        | BigWord of dat:bigint * width:int 

    let rec bitsToInt (lst:Bit list) =
        match lst with
        | [] -> 0u
        | x :: rest -> (if x = Zero then 0u else 1u) + 2u * bitsToInt rest

    let rec bitsToBig (lst:Bit list) =
        match lst with
        | [] -> bigint 0
        | x :: rest -> (if x = Zero then bigint 0 else bigint 1) + ((bitsToBig rest) <<< 1)
      

    let rec wireToFast (w: WireData) =
        let n = w.Length
        match w with
        | [Zero] -> Bit 0u
        | [One] -> Bit 1u
        | w when n <= 32 -> Word (bitsToInt w, w.Length)
        | w -> BigWord(bitsToBig w, n)
    
    [<Erase>]
    type InputPortNumber = | InputPortNumber of int

    [<Erase>]
    type OutputPortNumber = | OutputPortNumber of int

    [<Erase>]
    type Epoch = | Epoch of int

    type FastComponent = {
        Inputs: (FastData array array * OutputPortNumber) array
        Outputs: FastData array array
        SimComponent: SimulationComponent
        accessPath: ComponentId list
        } with
        member inline this.getInput (Epoch epoch)  (InputPortNumber n) = let a, (OutputPortNumber index) = this.Inputs.[n]
                                                                         a.[epoch].[index]
        member inline this.putOutput (Epoch epoch) (OutputPortNumber n) dat = this.Outputs.[epoch].[n] <- dat
        member inline this.Id = this.SimComponent.Id
                 
    // The fast simulation components are similar to the issie conmponents they are based on but with addition of arrays
    // for direct lookup of inputs an fast access of outputs. The input arrays contain pointers to the output arrays the
    // inputs are connected to, the InputportNumber integer indexes this.
    // In addition outputs are contained in a big array indexed by epoch (simulation time). This allows results for multiple
    // steps to begin built efficiently and also allows clocked outputs for the next cycle to be constructed without overwriting
    // previous outputs needed for that construction.
    //
    // For reasons of efficiency Issie's list-style WireData type is optimised by using integers as bit arrays.
    //
    // For ease of implementation Input and Output components are given a single output (input) port not present on issie.
    // this allows sub-sheet I/Os to be linked as normal in the constructed graph via their respective Input and Output connections.
    //
    // Although keeping input and output connections in the new graph is slightly less efficient it makes things simpler because there is a
    // 1-1 connection between components (except for custom components which are removed by the gathering process).
    // Note that custom component info is still kept because each component in the graph has a path - the list of custom component ids
    // between its graph and root. Following issie this does not include a custom component for the sheet being simulated, which is viewed as
    // root. Since custom components have been removed this no longer complicates the simulation.

    type FastSimulation = {
        Epoch: Epoch
        SimulationInputs: FastData array
        ClockedComponents: FastComponent array
        CombinationalComponents: FastComponent array
        }

    type GatherData = {
        /// existing Issie data structure representing circuit for simulation - generated by runCanvasStateChecksAndBuildGraph
        Simulation: SimulationGraph
        InputLinks: Map<ComponentId,ComponentId * InputPortNumber>
        OutputLinks: Map<ComponentId * OutputPortNumber, ComponentId>
        AllComponents: Map<ComponentId,SimulationComponent * ComponentId list> // maps to component and its path in the graph
        GInputs: ComponentId list
        OrderedComponents: ComponentId list
        }
    /// Create an initial gatherdata object with inputs, non-ordered components, simulationgraph, etc
    /// This must explore graph recursively extracting all the initial information.
    /// Custom components are scanned and links added, one for each input and output
    let startGather (graph: SimulationGraph) : GatherData = failwithf "Not implemented"

    /// Add components in order (starting with clocked components and inputs).
    /// The gathering process iteratively extracts components from AllComponents and adds
    /// them to orderedComponents such that a later component depends only on earlier components.
    /// Therefore evaluation can be done without checking inputs in reverse order of this list.
    let gatherComponents (gd: GatherData): GatherData = failwithf "Not Implemented"

    /// create a fast simulation data structure, with all necessary arrays, and components
    /// ordered for evaluation.
    /// this function also creates the reducer functions for each component
    /// similar to the reducer builder in Builder, but with inputs and outputs using the FastSimulation
    /// mutable arrays
    let buildFastSimulation (numberOfEpochs: int) (gd: GatherData) : FastSimulation = failwithf "Not Implemented"

    /// converts from WireData to FastData and sets simulation inputs from SimulationGraph
    let setSimulationData (fSim: FastSimulation) (graph: SimulationGraph) = failwithf "Not Implemented"

    /// write Simulation data back to an Issie structure.
    let writeSimulationData (fSim: FastSimulation) (epoch: Epoch) (graph: SimulationGraph) : SimulationGraph = failwithf "Not Implemented"

    /// run a simulation for a given number of steps
    let runSimulation (steps: int) (fSim: FastSimulation) : FastSimulation = failwithf "Not Implemented"
    
