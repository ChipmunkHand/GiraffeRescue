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
    
type TreatzState =
    { PressedKeys : Set<ScanCode> 
      Chaos : System.Random 
      mutable GameState : Game
      textures : Map<string, SDLTexture.Texture> 
      Controllers : Set<ControllerButton> * Set<ControllerButton>
      Sprites:     Map<byte,Rectangle>;
       }

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
        //blt state.textures.["background"] None
        
       
        ()

    // clear screen
    context.Renderer |> SDLRender.setDrawColor (0uy,0uy,50uy,0uy) |> ignore
    context.Renderer |> SDLRender.clear |> ignore

    context.Surface
    |> SDLSurface.fillRect None {Red=0uy;Green=0uy;Blue=0uy;Alpha=255uy}
    |> ignore
            
    context.Texture
    |> SDLTexture.update None context.Surface
    |> ignore
    context.Renderer |> SDLRender.copy context.Texture None None |> ignore

    match state.GameState.State with
    | Playing -> playing() 
    | GameOver -> () 


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
//                ("background",loadTex @"..\..\..\..\images\bg.bmp" )
//                ("cloud",loadTex @"..\..\..\..\images\cloud.bmp" )
//                ("catbus",loadTex @"..\..\..\..\images\catbus.bmp" )
//                ("boot",loadTex @"..\..\..\..\images\boot.bmp" )
//                ("cat-falling",loadTex @"..\..\..\..\images\cat-falling.bmp" )
//                ("cat-parachute",loadTex @"..\..\..\..\images\cat-parachute.bmp" )
//                ("font", loadTex @"..\..\..\..\images\romfont8x8.bmp")           
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