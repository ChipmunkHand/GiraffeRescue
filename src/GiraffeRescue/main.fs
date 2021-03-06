module Treatz_QT
open System
open SDLUtility
open SDLGeometry
open SDLPixel
open SDLRender
open SDLKeyboard
open Logic
let fps = 60.0;
let delay_time = 1000.0 / fps;
let delay_timei = uint32 delay_time

let screenWidth = 800<px>
let screenHeight = 600<px>

let cellWidth = 5
let cellHeight = 5

let mapWidth = 160
let mapHeight = 120

//type buonds = { x : int; y : int; width : int; height : int }
//let music =  
//    let p  = new System.Media.SoundPlayer(@"..\..\..\..\images\music.wav")
//   
//    fun () -> p.PlayLooping()


type ControllerButton =
   | BUTTON_A = 0
   | BUTTON_B = 1
   | BUTTON_X = 2
   | BUTTON_Y = 3
   | BUTTON_BACK = 4
   | BUTTON_GUIDE = 5
   | BUTTON_START = 6
   | BUTTON_LEFTSTICK = 7
   | BUTTON_RIGHTSTICK = 8
   | BUTTON_LEFTSHOULDER = 9
   | BUTTON_RIGHTSHOULDER = 10
   | BUTTON_DPAD_UP = 11
   | BUTTON_DPAD_DOWN = 12
   | BUTTON_DPAD_LEFT = 13
   | BUTTON_DPAD_RIGHT = 14

//type GameState =
//    TitleScreen 
//    | P1Win
//    | P2Win
//    | Playing
//    | Nope // both Splat (show splat?)
 
let inline bresenham fill (x0, y0) (x1, y1) =
    let steep = abs(y1 - y0) > abs(x1 - x0)
    let x0, y0, x1, y1 =
        if steep then y0, x0, y1, x1 else x0, y0, x1, y1
    let x0, y0, x1, y1 =
        if x0 > x1 then x1, y1, x0, y0 else x0, y0, x1, y1
    let dx, dy = x1 - x0, abs(y1 - y0)
    let s = if y0 < y1 then 1 else -1
    let rec loop e x y =
        if x <= x1 then
            if steep then fill y x else fill x y
            if e < dy then
                loop (e-dy+dx) (x+1) (y+s)
            else
                loop (e-dy) (x+1) y
    loop (dx/2) x0 y0
    
type TreatzState =
    { PressedKeys : Set<ScanCode> 
      Chaos : System.Random 
      mutable GameState : Game
      textures : Map<string, SDLTexture.Texture> 
      Controllers : Set<ControllerButton> * Set<ControllerButton>
      Sprites:     Map<byte,Rectangle>; }

let treeDepth = 15
type RenderingContext =
    {Renderer:SDLRender.Renderer;
     Texture:SDLTexture.Texture;
     Surface:SDLSurface.Surface;
     mutable lastFrameTick : uint32 }

let update (state:TreatzState) : TreatzState =
    let pressed (code:ScanCode) = if state.PressedKeys.Contains code then true else false
    let update (scancode, f) state = if pressed scancode then f state else state

    
//    let mapping = [(ControllerButton.BUTTON_A, Fire);(ControllerButton.BUTTON_DPAD_LEFT, Left);(ControllerButton.BUTTON_DPAD_RIGHT, Right)]
//
//    let getController buttons = 
//        buttons
//        |> Set.toList 
//        |> List.choose(fun b -> 
//            List.tryPick(fun (x,y) -> if x = b then Some y else None) mapping)
//        |> Set.ofList
//
//    state.GameState.Player1.buttonsPressed <- getController (fst state.Controllers)      
//    state.GameState.Player2.buttonsPressed <- getController (snd state.Controllers)      
//    
//  //  if state.GameState.Player1.buttonsPressed.IsEmpty |> not then System.Diagnostics.Debugger.Break()
//    if pressed ScanCode.S then state.GameState.NeckStart.y <- state.GameState.NeckStart.y + 5.
//    if pressed ScanCode.W the n state.GameState.NeckStart.y <- state.GameState.NeckStart.y - 5.
    if pressed ScanCode.D then 
        state.GameState.NeckStart.x <- state.GameState.NeckStart.x + 5.
        if state.GameState.NeckStart.x > 800.0 - 50.0 then
            state.GameState.NeckStart.x <- 800.0 - 50.0
  
    if pressed ScanCode.A then 
        state.GameState.NeckStart.x <- state.GameState.NeckStart.x - 5.
        if state.GameState.NeckStart.x < 50.0 then
            state.GameState.NeckStart.x <- 50.0


    
    //if pressed ScanCode.Down then state.GameState.NeckLength <- state.GameState.NeckLength - 5    
    if pressed ScanCode.Up then state.GameState.NeckLength <- state.GameState.NeckLength + 5    
    if pressed ScanCode.Right then 
        state.GameState.NeckAngle <- state.GameState.NeckAngle + 1.5
        if state.GameState.NeckAngle > 360.0 then state.GameState.NeckAngle <- 360.0

    if pressed ScanCode.Left then 
        state.GameState.NeckAngle <- state.GameState.NeckAngle - 1.5
        if state.GameState.NeckAngle < 180.0 then state.GameState.NeckAngle <- 180.0
  
    //if state.GameState.NeckAngle < 180.0 then state.GameState.NeckAngle <- 180.0

    state.GameState <- Logic.update state.GameState
    state
    

let rec eventPump (renderHandler:'TState->unit) (eventHandler:SDLEvent.Event->'TState->'TState option) (update:'TState->'TState) (state:'TState) : unit =
    match SDLEvent.pollEvent() with
    | Some event ->
        match state |> eventHandler event with
        | Some newState -> eventPump renderHandler eventHandler update newState
        | None -> ()
    | None -> 
        let state = update state
        state
        |> renderHandler
        eventPump renderHandler eventHandler update state


let handleEvent (event:SDLEvent.Event) (state:TreatzState) : TreatzState option =
    match event with
    | SDLEvent.ControllerButtonDown event  ->
        if event.Which = 0 then 
            Some({ state with Controllers = Set.add (enum<ControllerButton>(int event.Button)) (fst state.Controllers), (snd state.Controllers) } )
        else
            Some({ state with Controllers = (fst state.Controllers), Set.add (enum<ControllerButton>(int event.Button))(snd state.Controllers) } )
    | SDLEvent.ControllerButtonUp event  ->
        if event.Which = 0 then 
            Some({ state with Controllers = Set.remove (enum<ControllerButton>(int event.Button)) (fst state.Controllers), (snd state.Controllers) } )
        else
            Some({ state with Controllers = (fst state.Controllers), Set.remove (enum<ControllerButton>(int event.Button))(snd state.Controllers) } )
    | SDLEvent.KeyDown keyDetails when keyDetails.Keysym.Scancode = ScanCode.Escape ->
        None
    | SDLEvent.Quit _ -> 
        None
    | SDLEvent.KeyDown keyDetails -> 
        Some( { state with PressedKeys = Set.add keyDetails.Keysym.Scancode state.PressedKeys} )
    | SDLEvent.KeyUp keyDetails -> 
        Some( { state with PressedKeys = Set.remove keyDetails.Keysym.Scancode state.PressedKeys} )
    | _ -> Some state
        // core logic function here
        
    


let render(context:RenderingContext) (state:TreatzState) =
    let blt tex dest =
        context.Renderer |> copy tex None dest |> ignore

    let bltEx tex dest angle flip =
        context.Renderer |> copyEx tex None dest angle 0 |> ignore
        
    let bltf src dest =
        context.Renderer |> copy state.textures.["font"] (Some src) (Some dest) |> ignore

    let drawString (s:string) (x,y) =
        let mutable i = 0
        for c in s do
            bltf (state.Sprites.[byte c]) ({X = (x + (i*16)) * 1<px>; Y = y * 1<px>; Width = 16<px>; Height = 16<px>}) 
            i <- i + 1
             
    let playerWin() player = ()


    let playing() =
        let cloudRect (cloud : Position * Size) : SDLGeometry.Rectangle =
            let l,s = cloud 
            { X = (int l.x) * 1<px>; Y = (int l.y) * 1<px>; Width = (int s.width) * 1<px>; Height = (int s.height) * 1<px>}

        // always draw the catbuses and boots
        blt state.textures.["background"] None
        
        let tree = state.textures.["tree"] 
        let treer = { X= treePositionLeft; Y = treePositionTop; Width = treeWidth * 1<px>; Height = treeHeight * 1<px>} |> Some 
        blt tree treer
         
        let platform = state.textures.["platform"] 
        let pr = Some(platformRectangle)
        blt platform pr

        let ban = state.textures.["bananas"] 
        let br = { X= ((Logic.screenWidth - 80.0) |> int) * 1<px> ; Y = 20 * 1<px>; Width = 64 * 1<px>; Height = 50 * 1<px>} |> Some

        blt ban br


        for food in state.GameState.Foods do
          let fruit = if (chaos.Next(0,2) < 1)  then state.textures.["fruit-1"] else state.textures.["fruit-2"]
          let fruitR = { X= ( int food.x) * 1<px> ; Y = (int food.y) * 1<px>; Width = (int fruitSize) * 1<px>; Height = (fruitSize|> int ) * 1<px>} |> Some 
          blt fruit fruitR
        ()

    // clear screen
    context.Renderer |> SDLRender.setDrawColor (0uy,0uy,50uy,0uy) |> ignore
    context.Renderer |> SDLRender.clear |> ignore

    context.Surface
    |> SDLSurface.fillRect None {Red=80uy;Green=80uy;Blue=200uy;Alpha=255uy}
    |> ignore
            
    context.Texture
    |> SDLTexture.update None context.Surface
    |> ignore
    context.Renderer |> SDLRender.copy context.Texture None None |> ignore

    match state.GameState.State with
    | Playing -> 
        playing() 
        let draw x y =
            let z = { X = (int x) * 1<px>; Y = (int y) * 1<px>; Width = (int headSize) * 1<px>; Height = (int headSize) * 1<px>}
            bltEx state.textures.["neck"] (Some <| z) (state.GameState.NeckAngle + 90.) (state.GameState.NeckAngle < 90.0)
    
        let x = (float state.GameState.NeckLength) * Math.Cos(state.GameState.NeckAngle * Math.PI / 180.)
        let y = (float state.GameState.NeckLength) * Math.Sin(state.GameState.NeckAngle * Math.PI / 180.)

        let x2 = (int  (state.GameState.NeckStart.x + x))
        let y2 = (int  (state.GameState.NeckStart.y + y))
  
        bresenham 
            draw 
            ((int state.GameState.NeckStart.x),(int state.GameState.NeckStart.y)) 
            ((int x2),(int y2))

        let r = { X = x2 * 1<px>; Y = y2 * 1<px>; 
                  Width = (int headSize) * 1<px>; Height = (int headSize) * 1<px>}
    
        bltEx state.textures.["head-right"] (Some <| r) (state.GameState.NeckAngle + 90.0) (state.GameState.NeckAngle < 90.0)

        let r = { X = (int state.GameState.NeckStart.x - 50) * 1<px>; Y = (int state.GameState.NeckStart.y) * 1<px>; 
                Width = (int bodyWidth) * 1<px>; Height = (int bodyHeight) * 1<px>}
        
        blt state.textures.["body"] (Some r)
    
        let monkeyR = { X = (int state.GameState.NeckStart.x - 10) * 1<px>; Y = (int state.GameState.NeckStart.y) * 1<px>; 
                Width = (20) * 1<px>; Height = (int 30) * 1<px>} 
        blt state.textures.["monkey"] (Some monkeyR)
        drawString "GIRAFFE RESCUE" (100, 10)
        let ts = (DateTime.Now - startTime)
        drawString ("TIME TAKEN " + (ts.Seconds.ToString()))  (500, 10)
    | GameOver ->  
        blt state.textures.["monkey-end"] None
        drawString ("WELL DONE! YOU SAVED THE MONKEY!")  (100, 10)
        drawString ("YOU TOOK " + (state.GameState.endScore.ToString()) + " SECONDS")  (150, 50)


  

    context.Renderer |> SDLRender.present 
    
    // delay to lock at 60fps (we could do extra work here)
    let frameTime = getTicks() - context.lastFrameTick
    if frameTime < delay_timei then delay(delay_timei - frameTime)
    context.lastFrameTick <- getTicks()    


    


let main() = 
    use system = new SDL.System(SDL.Init.Video ||| SDL.Init.Events ||| SDL.Init.GameController)
    use mainWindow = SDLWindow.create "test" 100<px> 100<px> screenWidth screenHeight (uint32 SDLWindow.Flags.Resizable) // FULLSCREEN!
    //use mainWindow = SDLWindow.create "test" 100<px> 100<px> screenWidth screenHeight (uint32 SDLWindow.Flags.FullScreen) // FULLSCREEN!    
    use mainRenderer = SDLRender.create mainWindow -1 SDLRender.Flags.Accelerated
    use surface = SDLSurface.createRGB (screenWidth,screenHeight,32<bit/px>) (0x00FF0000u,0x0000FF00u,0x000000FFu,0x00000000u)    
    use mainTexture = mainRenderer |> SDLTexture.create SDLPixel.RGB888Format SDLTexture.Access.Streaming (screenWidth,screenHeight)
    mainRenderer |> SDLRender.setLogicalSize (screenWidth,screenHeight) |> ignore
    
    SDLGameController.gameControllerOpen 0
    SDLGameController.gameControllerOpen 1
    
    let context =  { Renderer = mainRenderer; Texture = mainTexture; Surface = surface; lastFrameTick = getTicks() }
    
    // create default state
    let setKey bitmap colour =    
        bitmap
        |> SDLSurface.setColorKey (Some colour)
        |> ignore   

    let state = 
        let magenta = {Red=255uy;Green=0uy;Blue=255uy;Alpha=0uy}
        let loadTex file =
            use bmp = SDLSurface.loadBmp SDLPixel.RGB888Format file
            setKey bmp magenta
            SDLTexture.fromSurface mainRenderer bmp.Pointer
        
        //use tittleScreenBitmap = SDLSurface.loadBmp SDLPixel.RGB888Format @"..\..\..\..\images\title.bmp"
        let tex = 
            [
//                ("titlescreen",loadTex @"..\..\..\..\images\title.bmp" )
                ("background",loadTex @"..\..\..\..\images\bg.bmp" )
                ("tree",loadTex @"..\..\..\..\images\tree.bmp" )
                ("fruit-1",loadTex @"..\..\..\..\images\fruit.bmp")
                ("fruit-2",loadTex @"..\..\..\..\images\fruit.bmp")
                ("neck",loadTex @"..\..\..\..\images\neck.bmp" )
                ("head-right",loadTex @"..\..\..\..\images\head-right.bmp" )
                ("body",loadTex @"..\..\..\..\images\body.bmp" )
                ("platform", loadTex @"..\..\..\..\images\platform.bmp")
                ("bananas", loadTex @"..\..\..\..\images\bannanas.bmp")
                ("monkey", loadTex @"..\..\..\..\images\monkey.bmp")
                ("monkey-end", loadTex @"..\..\..\..\images\end-moneky.bmp")

                ("font", loadTex @"..\..\..\..\images\romfont8x8.bmp")           
            ] |> Map.ofList
        
        use bitmap = SDLSurface.loadBmp SDLPixel.RGB888Format @"..\..\..\..\images\romfont8x8.bmp"

        bitmap
        |> SDLSurface.setColorKey (Some {Red=255uy;Green=255uy;Blue=255uy;Alpha=0uy})
        |> ignore

        
        let sprites = 
            [0uy..255uy]
            |> Seq.map(fun index-> (index, ( {X=8<px>*((index |> int) % 16); Y=8<px>*((index |> int) / 16);Width=8<px>;Height=8<px>})))
            |> Map.ofSeq
    
        //let sprite = context.Sprites.[cell.Character]
//         sprite
//        |>* blitSprite {X=0<px>;Y=0<px>} context.WorkSurface

        //music()
        {Chaos = System.Random()
         PressedKeys = Set.empty
         Sprites = sprites
         GameState = StartGame()
         textures = tex
         Controllers = Set.empty, Set.empty
         }

    eventPump (render context) handleEvent update state

main()