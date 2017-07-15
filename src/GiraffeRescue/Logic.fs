module Logic
open SDLUtility
open System
open SDLGeometry

let chaos = System.Random(DateTime.Now.Millisecond)
let startTime = DateTime.Now
let screenWidth = 800.0
let screenHeight = 600.0

let maxFruits = 20


let fruitSize = 20.0
let headSize = 40.0
let neckIncreaseAmount = 10
let bodyWidth = 125
let bodyHeight = 100

let treePositionTop =   1<px>
let treePositionLeft =  50 * 1<px> 
let treeHeight = int screenHeight - 50
let treeWidth =  int screenWidth - 100


let treeTrunk = 500
let bottomTree = 400 
type Size = 
    {
        width : float
        height : float 
    }

type Position = 
    {
        mutable x : float
        mutable y : float
        mutable vx : float
        mutable vy : float
    } 
    with 
    static member Zero() =
        { x = 0.; y = 0.; vx=0.; vy = 0. }
    member this.Update() = 
            this.x <- this.x + this.vx
            this.y <- this.y + this.vy

type GameState =
    | Playing
    | GameOver
           
type Game =
    {
        mutable State : GameState
        mutable NeckStart : Position
        //mutable NeckEnd: Position
        mutable NeckLength : int
        mutable NeckAngle : float
        mutable Foods : ResizeArray<Position>
        mutable MaxFood : int
        mutable lastShrink : int
        mutable lastSpawn : int
    }


let StartGame() =
    let state = 
        let foods = ResizeArray<_>()
        { x =   screenWidth - 150. 
          y =  treeTrunk |> float
          vx = 0.
          vy = 0.} |> foods.Add
        
        for n in 1..maxFruits do
            { x =  chaos.Next(60, int (screenWidth - 100.)) |> float  
              y =  chaos.Next(50, int treeTrunk) |> float
              vx = 0.
              vy = 0.} |> foods.Add

        {            
            State = Playing
            NeckStart = Position.Zero()
            NeckAngle = 360.0
            NeckLength = 50
            Foods = foods
            MaxFood =  maxFruits
            lastShrink = 0
            lastSpawn = 0
        }
    state.NeckStart.y <- 505.0
    state

let overlap(rectA, rectB) =
    let x1 = rectA.X
    let x2 = rectA.X + rectA.Width
    let y1 = rectA.Y
    let y2 = rectA.Y + rectA.Height

    let x1' = rectB.X
    let x2' = rectB.X + rectB.Width
    let y1' = rectB.Y
    let y2' = rectB.Y + rectB.Height

    x2' >= x1 && x1' <= x2 && y2' >= y1 && y1' <= y2

let processCollisions (state:Game) =
    let mutable i = state.Foods.Count - 1
    let x = (float state.NeckLength) * Math.Cos(state.NeckAngle * Math.PI / 180.)
    let y = (float state.NeckLength) * Math.Sin(state.NeckAngle * Math.PI / 180.)

    let x2 = (int  (state.NeckStart.x + x))
    let y2 = (int  (state.NeckStart.y + y))
//
    while i > -1 do
        let food = state.Foods.[i]
        let r1 = { X = (int food.x) * 1<px>; Y = (int food.y) * 1<px>; Width = (int fruitSize) * 1<px>; Height = (int fruitSize) * 1<px>}
        let r2 = { X = (int x2 + 10) * 1<px>; Y = (int y2) * 1<px>; Width = (int headSize - 10) * 1<px>; Height = (int headSize - 20) * 1<px>}
        if overlap(r1, r2) then
            state.Foods.RemoveAt(i)
            state.NeckLength <- state.NeckLength + neckIncreaseAmount
        i <- i - 1
        ()
    


    
let update (state:Game) =
    processCollisions state
    
    

    let ts = DateTime.Now - startTime    
    let elapsed = (int ts.TotalSeconds)
    if elapsed % 3 = 0 && elapsed > state.lastShrink then
        state.lastShrink <- elapsed
        if state.NeckLength > 10 then
            state.NeckLength <- state.NeckLength - 10

    if elapsed % 1 = 0 && elapsed > state.lastSpawn then
        state.lastSpawn <- elapsed
        let f =
              { x =  chaos.Next(60, int (screenWidth - 100.)) |> float  
                y =  chaos.Next(50, int treeTrunk) |> float
                vx = 0.
                vy = 0.}
        state.Foods.Add f
    
    state
    
